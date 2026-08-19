using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;

using FancyMouse.Common.Capture;
using FancyMouse.Common.Helpers;
using FancyMouse.Common.Telemetry;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.WinUI3.Internal.Helpers;

using Microsoft.UI.Windowing;

using Windows.Graphics;

namespace FancyMouse.WinUI3.UI;

public sealed partial class PreviewWindow
{
    public async Task ShowPreviewAsync(CancellationToken cancellationToken)
    {
        var logger = this.Logger;

        logger.Info(string.Join(
            '\n',
            "-----------",
            nameof(PreviewWindow.ShowPreviewAsync),
            "-----------"));

        // claim the foreground window as early as possible - see ClaimForegroundWindow
        // remarks for why this needs to happen now, before any of the capture/layout work
        // below, rather than later when we actually show the window
        this.SetAsForegroundWindow();

        using var activationTimer = Telemetry.Current.BeginTimer(new { }, nameof(PreviewWindow.ShowPreviewAsync));

        // hide the form while we redraw it... (also cancels whatever the *previous* activation's
        // own activationCancellation was, via ClearPreview - harmless if the caller already
        // cancelled and awaited it before calling this method, since cancelling an
        // already-cancelled source is a no-op)
        using (Telemetry.Current.BeginTimer(new { }, "HideWindowAsync"))
        {
            await this.HideWindowAsync()
                .ConfigureAwait(false);
        }

        // capture this first so we get an accurate current mouse location
        // (in case the user moves it a few pixels while the form is rendered)
        var activatedLocation = MouseHelper.GetCursorPosition();

        var appSettings = ConfigHelper.AppSettings ?? throw new InvalidOperationException();

        DisplayInfo displayInfo;
        using (Telemetry.Current.BeginTimer(new { }, "GetDisplayInfo"))
        {
            displayInfo = DeviceHelper.GetDisplayInfo();
        }

        var activatedScreen = DeviceHelper.GetActivatedScreen(displayInfo.Devices[0], activatedLocation);

        var previewStyle = appSettings.PreviewStyle;
        PreviewLayout previewLayout;
        using (Telemetry.Current.BeginTimer(new { }, "GetPreviewLayout"))
        {
            previewLayout = LayoutHelper.GetPreviewLayout(
                previewStyle,
                displayInfo,
                activatedScreen: activatedScreen);
        }

        // the outer border is this window's own responsibility, not the preview pane's -
        // see LayoutHelper.GetHostBoxStyle. PreviewLayout itself has no desktop position
        // (only a size - see PreviewLayout), so positioning the window on the desktop -
        // centered on the activated location, clamped to the activated screen - is entirely
        // this window's own job too.
        var hostBoxStyle = LayoutHelper.GetHostBoxStyle(previewStyle.CanvasStyle);
        var hostBounds = LayoutHelper.GetHostBounds(new RectangleInfo(previewLayout.PreviewSize), hostBoxStyle);
        var positionedHostOuterBounds = LayoutHelper.PositionOnScreen(hostBounds.OuterBounds, activatedScreen, activatedLocation);

        // a newer activation superseding this one is the common, expected case under rapid
        // repeat activation - check here, before paying for the border render below, rather
        // than only ever noticing via a later awaited call
        cancellationToken.ThrowIfCancellationRequested();

        using (Telemetry.Current.BeginTimer(new { }, "PositionWindowAsync"))
        {
            await this.PositionWindowAsync(positionedHostOuterBounds)
                .ConfigureAwait(false);
        }

        (int Width, int Height, int CornerRadius) windowRegion;
        using (Telemetry.Current.BeginTimer(new { }, "RenderBorderAsync"))
        {
            windowRegion = await this.RenderBorderAsync(previewLayout, hostBoxStyle)
                .ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // builds every screen's bezel + placeholder fill immediately, so there's something to
        // show as soon as the window becomes visible - screenshots backfill afterwards
        using (Telemetry.Current.BeginTimer(new { }, "SetPreviewPaneLayoutAsync"))
        {
            await this.SetPreviewPaneLayoutAsync(previewLayout, activatedScreen)
                .ConfigureAwait(false);
        }

        // start a fresh cancellation scope right before it's first needed, rather than earlier
        // in the method - the gap between creating this and using it is exactly the window in
        // which a spurious PreviewWindow_Activated Deactivated event (e.g. from moving/hiding
        // the window above) could call ClearPreview and dispose it out from under us, so keep
        // that gap as small as possible. Linked to the caller's token, so either the caller
        // superseding this activation *or* the preview being cleared for any other reason (see
        // ClearPreview) stops the capture pipeline below.
        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        this.activationCancellation = cancellation;

        var pipeline = new ScreenshotCapturePipeline(new PreviewPaneScreenshotSink(this), cancellation.Token);

        // one capture provider per device - see IScreenshotCaptureProvider/
        // DesktopScreenshotCaptureProvider remarks for why a single instance is safe (and
        // necessary) to share across all of that device's screens. Ownership of each provider
        // transfers to the pipeline - see ScreenshotCapturePipeline.DisposeAsync.
        var captureTasks = new List<(ScreenLayout ScreenLayout, Task<Bitmap> CaptureTask)>();
        using (Telemetry.Current.BeginTimer(new { }, "kickOffCaptureRequests"))
        {
            foreach (var deviceLayout in previewLayout.CanvasLayout.DeviceLayouts)
            {
                captureTasks.AddRange(pipeline.AddCaptureTasks(
                    deviceLayout, new DesktopScreenshotCaptureProvider()));
            }
        }

        // the activated screen's own capture must complete before the window is shown,
        // otherwise a *later* capture of that screen would risk capturing the preview window
        // itself, since it's positioned on top of the activated screen. We only need the
        // capture itself to be done here, not for it to have reached the PreviewPane yet - the
        // pipeline pushes every result to the pane independently, in the background.
        var activatedCaptureTask = captureTasks
            .Single(entry => object.ReferenceEquals(entry.ScreenLayout.ScreenInfo, activatedScreen))
            .CaptureTask;

        try
        {
            // give every screen up to ScreenshotGracePeriod to finish capturing before
            // showing the window - a typical (fast, local) activation finishes well within
            // that and shows fully populated, with none of the placeholder-then-backfill
            // repainting a reader would otherwise see. A screen that's still slow after that
            // (e.g. a future remote capture provider) doesn't hold the window hostage though
            // - it just backfills afterwards, same as it would have anyway. The activated
            // screen's own capture is still awaited separately below regardless of which way
            // the race went, since that one's non-negotiable (see above).
            var allCaptureTasks = captureTasks.Select(entry => entry.CaptureTask).ToArray();
            using (Telemetry.Current.BeginTimer(new { }, "gracePeriodRace"))
            {
                await Task.WhenAny(Task.WhenAll(allCaptureTasks), Task.Delay(PreviewWindow.ScreenshotGracePeriod, cancellation.Token))
                    .ConfigureAwait(false);
            }

            using (Telemetry.Current.BeginTimer(new { }, "activatedScreenCapture"))
            {
                await activatedCaptureTask
                    .ConfigureAwait(false);
            }

            using (Telemetry.Current.BeginTimer(new { }, "ShowWindowAsync"))
            {
                await this.ShowWindowAsync(windowRegion.Width, windowRegion.Height, windowRegion.CornerRadius, cancellation.Token)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // a newer activation superseded this one while we were still waiting on captures
            // - let it own showing the window instead of us doing so with what's now stale
            // layout/position state
        }
        catch (Exception ex)
        {
            // a genuine (non-cancellation) failure capturing the activated screen - unlike a
            // backfill screen's failure (see ObserveAndDisposeAsync), this one has to stop us
            // from showing the window at all, since we can't guarantee it's safe to reveal
            // without knowing that screen was actually captured
            logger.Error(ex, $"failed to capture the activated screen ({activatedScreen}); not showing the preview window");
        }
        finally
        {
            // don't hold ShowPreviewAsync open waiting for every screen to finish
            // backfilling - ObserveAndDisposeAsync logs any backfill capture failure and
            // disposes the pipeline's providers once everything's settled, whether or not we
            // ended up showing the window above
            _ = this.ObserveAndDisposeAsync(pipeline);
        }
    }

    private void ClearPreview()
    {
        this.activationCancellation?.Cancel();
        this.activationCancellation?.Dispose();
        this.activationCancellation = null;

        if ((this.BorderImage.Source is null) && (this.PreviewPane.Layout is null))
        {
            return;
        }

        this.BorderImage.Source = null;
        this.PreviewPane.Layout = null;
        this.PreviewPane.ActiveScreen = null;
    }

    private void HideWindow()
    {
        // clip to an empty region rather than AppWindow.Hide() - see ApplyEmptyRectRegion's remarks
        this.ApplyEmptyRectRegion();
        this.ClearPreview();
    }

    private async Task HideWindowAsync()
    {
        await this.InvokeOnUiThreadAsync(
            () =>
            {
                this.HideWindow();
            }).ConfigureAwait(false);
    }

    /// <summary>
    /// Reveals the window by swapping its clip region from empty (see <see cref="HideWindow"/>) to
    /// <paramref name="width"/> x <paramref name="height"/> with corner radius
    /// <paramref name="cornerRadius"/> - the same values <see cref="RenderBorderAsync"/> rendered
    /// against, passed back in here rather than reapplied as part of that method, so the region
    /// only ever shows real, fully-built content instead of becoming visible partway through
    /// rendering it.
    /// </summary>
    private async Task ShowWindowAsync(int width, int height, int cornerRadius, CancellationToken cancellationToken)
    {
        await this.InvokeOnUiThreadAsync(
            () =>
            {
                // the activation can be cancelled (e.g. a right-click) in the gap between
                // this callback being queued via DispatcherQueue.TryEnqueue and it actually
                // running - that queuing is asynchronous even when called from the UI thread,
                // while HideWindow/ClearPreview run synchronously the moment a right-click is
                // handled, so without this check a queued reveal could still run *after* the
                // content it's about to reveal has already been wiped, showing an empty,
                // unstyled window instead of leaving it hidden
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var presenter = this.AppWindow.Presenter as OverlappedPresenter
                    ?? throw new InvalidOperationException();

                // we seem to need to turn off topmost and then re-enable it again
                // when we show the form - otherwise it doesn't always get shown topmost...
                presenter.IsAlwaysOnTop = false;
                presenter.IsAlwaysOnTop = true;

                this.ApplyRoundRectRegion(width, height, cornerRadius);

                // we have to activate the window to make sure the deactivate event fires
                this.Activate();
                this.FocusPreviewPane();
            }).ConfigureAwait(false);
    }

    /// <summary>
    /// Resize and position the form.
    /// </summary>
    private async Task PositionWindowAsync(RectangleInfo bounds)
    {
        await this.InvokeOnUiThreadAsync(
            () =>
            {
                // note - do this with two calls to MoveAndResize rather than one as there appears to
                // be an issue with dpi scaling even when using PerMonitorV2, where if the window is
                // resized *and* moved in one call the resize uses the scaling of the *current*
                // monitor before it's moved.
                //
                // If the move then happens to be to a different monitor, *and* the monitor has a
                // different dpi scaling configured, the window size is then wrong for the dpi scaling
                // of the new monitor.
                //
                // the workaround seems to be to call MoveAndResize twice - the first call might
                // resize it incorrectly, but it moves the window to the correct monitor, and the
                // second call then resizes it correctly.
                //
                // see https://github.com/mikeclayton/FancyMouse/issues/2 for more details
                var windowBounds = new RectInt32((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
                this.AppWindow.MoveAndResize(windowBounds);
                this.AppWindow.MoveAndResize(windowBounds);
            }).ConfigureAwait(false);
    }
}
