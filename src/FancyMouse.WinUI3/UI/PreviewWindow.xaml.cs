using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

using FancyMouse.Common.Capture;
using FancyMouse.Common.Helpers;
using FancyMouse.Common.Telemetry;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;
using FancyMouse.WinUI3.Internal.Helpers;
using FancyMouse.WinUI3.Win32Gen;

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace FancyMouse.WinUI3.UI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PreviewWindow : Window
{
    public PreviewWindow(NLog.ILogger logger)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.InitializeComponent();
        this.InitializeWindow();
    }

    private NLog.ILogger Logger
    {
        get;
    }

    /// <summary>
    /// Governs every screenshot capture started by the current activation's
    /// <see cref="ScreenshotCapturePipeline"/> - linked to the <see cref="CancellationToken"/>
    /// <see cref="ShowPreviewAsync"/> was called with (see its remarks for what that guards
    /// against), and *also* cancelled whenever the preview is cleared for any other reason (see
    /// <see cref="ClearPreview"/> - e.g. the user dismissed the window while backfill captures
    /// were still running), so outstanding captures for a no-longer-relevant activation don't
    /// keep doing GDI work in the background either way.
    /// </summary>
    private CancellationTokenSource? activationCancellation;

    /// <summary>
    /// Maximum time to wait for every screen's capture to finish before showing the window
    /// anyway - long enough that a typical (fast, local) activation shows fully populated with
    /// no visible placeholder-then-backfill repainting, short enough that one slow screen (e.g.
    /// a future remote capture provider) can't make the window feel unresponsive. Matches the
    /// grace period the legacy WinForms version used.
    /// </summary>
    private static readonly TimeSpan ScreenshotGracePeriod = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// This window's own handle - captured once in <see cref="InitializeWindow"/> and reused
    /// wherever later code needs to talk to the real Win32 window (<see cref="ApplyWindowRegion"/>
    /// in particular), rather than re-resolving it from <see cref="WinRT.Interop.WindowNative"/>
    /// every time.
    /// </summary>
    private HWND hWnd;

    /// <summary>
    /// Initializes some settings on the application window.
    /// </summary>
    private void InitializeWindow()
    {
        this.hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);

        var appWindow = this.AppWindow;
        var presenter = appWindow.Presenter as OverlappedPresenter;
        if (presenter != null)
        {
            // get the current window style
            var result = User32.GetWindowLong(this.hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE)
                .ThrowIfFailed()
                .GetValue();

            // set the window to be borderless, with no title bar, and hide all of the max / min / close buttons
            var style = (WINDOW_STYLE)result;
            style &= ~WINDOW_STYLE.WS_OVERLAPPEDWINDOW;
            style |= WINDOW_STYLE.WS_POPUP;
            _ = User32.SetWindowLong(this.hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style)
                .ThrowIfFailed();

            // get the current extended window style
            result = User32.GetWindowLong(this.hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE)
                .ThrowIfFailed()
                .GetValue();

            // set the window to be borderless, with no title bar, and hide all of the max / min / close buttons
            var exStyle = (WINDOW_EX_STYLE)result;
            exStyle |= WINDOW_EX_STYLE.WS_EX_TOOLWINDOW; // hide the taskbar icon
            exStyle |= WINDOW_EX_STYLE.WS_EX_TOPMOST;    // make topmost
            _ = User32.SetWindowLong(this.hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)exStyle)
                .ThrowIfFailed();
        }

        this.Activated += this.PreviewWindow_Activated;
        this.PreviewPane.NavigateTo += this.PreviewPane_NavigateTo;
        this.PreviewPane.Cancel += this.PreviewPane_Cancel;

        // this window is never actually Hide()/Show()'d again after this point - "hidden" is
        // instead an empty SetWindowRgn clip (see ClipWindowToNothing/HideWindow), so DWM keeps
        // compositing it continuously in the background instead of presenting a blank first
        // frame on every reveal. Clip to nothing *before* ever showing it, so there's no
        // on-screen flash of its default (unstyled) content at startup either.
        this.ClipWindowToNothing();
        this.AppWindow.Show();
    }

    /// <summary>
    /// Clips this window itself - not just its content - to a rounded-rectangle region sized
    /// <paramref name="width"/> x <paramref name="height"/> with corner radius
    /// <paramref name="cornerRadius"/>, matching <see cref="DrawingHelper.RenderBorder"/>'s own
    /// outer bezel shape (see <c>BezelRenderer</c>'s remarks: bezel thickness sets both the ring
    /// width and the outer corner radius). Pixels outside the region are never composited by the
    /// OS at all - genuinely showing the real desktop through them, unlike
    /// <see cref="BorderImage"/>'s own transparent corners, which an unpackaged WinUI3 window
    /// can't turn into real per-pixel alpha against the desktop on their own.
    /// </summary>
    /// <remarks>
    /// Called by <see cref="ShowWindowAsync"/> with the real, just-rendered size to reveal the
    /// window - see <see cref="ClipWindowToNothing"/> for the "hidden" counterpart. Since the
    /// window is never really Hide()/Show()'d after <see cref="InitializeWindow"/>'s first call,
    /// there's no "first frame" to flash blank on reveal - only a cheap region swap.
    /// <see cref="Gdi32.CreateRoundRectRgn"/>'s region handle is only freed by this method on
    /// failure - <c>SetWindowRgn</c> takes ownership of it on success, and deleting it afterwards
    /// would be a use-after-free from the OS's perspective.
    /// </remarks>
    private void ApplyWindowRegion(int width, int height, int cornerRadius)
    {
        // CreateRoundRectRgn's bottom-right point is exclusive (the region covers columns
        // [0, x2) and rows [0, y2), not [0, x2]) - confirmed by disabling SetWindowRgn entirely
        // and finding the window's own rendering was already correct and complete right up to
        // its true edge. Using width/height directly here was clipping away that genuinely
        // rendered last row/column; +1 on each makes the region cover the full width x height
        // window instead of width-1 x height-1 of it.
        var region = Gdi32.CreateRoundRectRgn(0, 0, width + 1, height + 1, cornerRadius * 2, cornerRadius * 2)
            .ThrowIfFailed()
            .GetValue();
        this.SetWindowRegion(region);
    }

    /// <summary>
    /// Clips this window to nothing at all, making it fully invisible while it stays actively
    /// composited by DWM - see <see cref="HideWindow"/>/<see cref="InitializeWindow"/>, and
    /// <see cref="ApplyWindowRegion"/>'s remarks for why that matters.
    /// </summary>
    /// <remarks>
    /// Deliberately <c>CreateRectRgn(0, 0, 0, 0)</c>, not <c>CreateRoundRectRgn</c> with all-zero
    /// arguments: only <c>CreateRectRgn</c> documents that setting both diametrically-opposite
    /// corners to (0,0) creates a genuinely empty region.
    /// <c>CreateRoundRectRgn</c> makes no such guarantee for a degenerate rectangle - it can
    /// return <see langword="null"/> (a real failure this codebase saw), rather than an empty
    /// region.
    /// </remarks>
    private void ClipWindowToNothing()
    {
        var region = Gdi32.CreateRectRgn(0, 0, 0, 0)
            .ThrowIfFailed()
            .GetValue();
        this.SetWindowRegion(region);
    }

    private void SetWindowRegion(HRGN region)
    {
        try
        {
            _ = User32.SetWindowRgn(this.hWnd, region, bRedraw: true)
                .ThrowIfFailed();
        }
        catch
        {
            _ = Gdi32.DeleteObject((HGDIOBJ)region)
                .IgnoreFailure();
            throw;
        }
    }

    private void PreviewWindow_Activated(object sender, WindowActivatedEventArgs e)
    {
        switch (e.WindowActivationState)
        {
            case WindowActivationState.CodeActivated:
                this.FocusPreviewPane();
                break;
            case WindowActivationState.Deactivated:
                this.HideWindow();
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Handles a navigation intent from <see cref="PreviewPane"/> - a screenshot click or the
    /// equivalent keyboard shortcut (see <see cref="PreviewPane.NavigateTo"/>). Only local
    /// devices exist today (see <see cref="Common.Helpers.DeviceHelper.GetDisplayInfo"/>), so
    /// this always moves the local cursor; <see cref="NavigateToEventArgs.Device"/> is there for
    /// when a remote (Mouse Without Borders) device needs routing elsewhere instead.
    /// </summary>
    private void PreviewPane_NavigateTo(object? sender, NavigateToEventArgs e)
    {
        var logger = this.Logger;

        logger.Info(string.Join(
            '\n',
            "-----------",
            nameof(PreviewWindow.PreviewPane_NavigateTo),
            "-----------",
            $"device   = {e.Device.Hostname}",
            $"location = {e.Location}"));

        MouseHelper.SetCursorPosition(e.Location);
        this.HideWindow();
    }

    private void PreviewPane_Cancel(object? sender, EventArgs e)
    {
        this.HideWindow();
    }

    /// <summary>
    /// Builds and shows the preview for a single activation. Doesn't guard against being called
    /// again before it's finished - that's deliberately the caller's job, not this method's (see
    /// the hotkey handler in <c>App.xaml.cs</c>): the caller is expected to cancel
    /// <paramref name="cancellationToken"/>'s source and wait for any previous call to this
    /// method to actually finish before starting a new one, so two calls can never be mutating
    /// this window's UI state at the same time. This method's own part of that contract is just
    /// to check <paramref name="cancellationToken"/> between its main steps and stop promptly
    /// (by throwing <see cref="OperationCanceledException"/>, same as any other cancellable
    /// async method) if it's been superseded, rather than finishing a layout nobody wants.
    /// </summary>
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
        this.ClaimForegroundWindow();

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
                await this.ShowWindowAsync(windowRegion.Width, windowRegion.Height, windowRegion.CornerRadius)
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

    /// <summary>
    /// Waits for every screenshot capture this activation's <paramref name="pipeline"/> started
    /// to finish, logging a failure if any of them - other than the activated screen, which is
    /// handled separately in <see cref="ShowPreviewAsync"/> since it has to stop the window from
    /// being shown at all - didn't succeed, then disposes the pipeline regardless.
    /// </summary>
    private async Task ObserveAndDisposeAsync(ScreenshotCapturePipeline pipeline)
    {
        try
        {
            await pipeline.WaitForCompletionAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Logger.Error(ex, "one or more screenshot captures failed");
        }
        finally
        {
            await pipeline.DisposeAsync().ConfigureAwait(false);
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

        // each activation churns through several WriteableBitmap-sized pixel buffers
        // (background, bezels, content) - releasing these references here just makes them
        // eligible for collection; the actual GC.Collect() call used to live here too, but
        // ClearPreview runs at the *start* of every activation (via HideWindow), which meant
        // forcing a collection right when the next activation needed CPU most. Moved to
        // App.xaml.cs's hotkey handler, after a successful ShowPreviewAsync completes - the
        // true end of user interaction, once there's no performance pressure left.
    }

    /// <summary>
    /// Calculates the high-dpi scaling ratio based on the current monitor's display settings.
    /// </summary>
    private double GetHighDpiScalingRatio()
    {
        var hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowDpi = User32.GetDpiForWindow(hWnd)
            .ThrowIfFailed()
            .GetValue();
        var scalingRatio = (double)PInvoke.USER_DEFAULT_SCREEN_DPI / windowDpi;
        return scalingRatio;
    }

    internal async Task InvokeOnUiThreadAsync(Action action)
    {
        // this might be called from a task that we're awaiting
        // so we need to make sure we use the UI thread
        var tcs = new TaskCompletionSource<bool>();

        this.DispatcherQueue.TryEnqueue(
            () =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

        // wait for the task to complete
        await tcs.Task.ConfigureAwait(false);
    }

    private void HideWindow()
    {
        // clip to nothing rather than AppWindow.Hide() - see ClipWindowToNothing's remarks
        this.ClipWindowToNothing();
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
    /// Reveals the window built by <see cref="RenderBorderAsync"/>/<see cref="SetPreviewPaneLayoutAsync"/>
    /// by swapping its clip region from empty (see <see cref="HideWindow"/>) to
    /// <paramref name="width"/> x <paramref name="height"/> with corner radius
    /// <paramref name="cornerRadius"/> - the same values <see cref="RenderBorderAsync"/> rendered
    /// against, passed back in here rather than reapplied as part of that method, so the region
    /// only ever shows real, fully-built content instead of becoming visible partway through
    /// rendering it.
    /// </summary>
    private async Task ShowWindowAsync(int width, int height, int cornerRadius)
    {
        await this.InvokeOnUiThreadAsync(
            () =>
            {
                var presenter = this.AppWindow.Presenter as OverlappedPresenter
                    ?? throw new InvalidOperationException();

                // we seem to need to turn off topmost and then re-enable it again
                // when we show the form - otherwise it doesn't always get shown topmost...
                presenter.IsAlwaysOnTop = false;
                presenter.IsAlwaysOnTop = true;

                this.ApplyWindowRegion(width, height, cornerRadius);

                // we have to activate the window to make sure the deactivate event fires
                this.Activate();
                this.FocusPreviewPane();
            }).ConfigureAwait(false);
    }

    /// <summary>
    /// Claims the OS foreground window - and with it, real keyboard focus - for this window.
    /// Windows silently refuses <see cref="SetForegroundWindow"/> for a background process
    /// unless (among other exemptions) that process "received the last input event". Unlike a
    /// typical background process, this one genuinely does: <see cref="FancyMouse.HotKeys.HotKeyManager"/>
    /// registers the global hotkey and receives <c>WM_HOTKEY</c> in-process (see its remarks),
    /// so this process is the one that just received real user input - no synthetic input
    /// needs simulating, unlike the more common workaround for this restriction. What matters
    /// is calling this the moment we're responding to that hotkey, before the exemption lapses
    /// - see the call at the top of <see cref="ShowPreviewAsync"/>, well before the
    /// capture/layout work that used to sit in front of this call and made it unreliable.
    /// </summary>
    private void ClaimForegroundWindow()
    {
        var hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);
        _ = User32.SetForegroundWindow(hWnd)
            .IgnoreFailure();
    }

    /// <summary>
    /// Sets keyboard focus on <see cref="PreviewPane"/>, which owns all keyboard navigation
    /// (see <see cref="PreviewPane.NavigateTo"/>) via its own <c>PreviewKeyDown</c> - unlike a
    /// window-level key handler, that only fires if focus actually lands inside the pane's own
    /// subtree, not just anywhere in the window. <c>Control.Focus</c> can fail silently (returns
    /// <see langword="false"/>) if the pane hasn't finished layout yet - most likely the very
    /// first time the window is ever shown after the app starts - so this retries once on the
    /// next UI thread tick rather than assuming the first attempt landed.
    /// </summary>
    private void FocusPreviewPane()
    {
        if (this.PreviewPane.Focus(FocusState.Programmatic))
        {
            return;
        }

        this.DispatcherQueue.TryEnqueue(() => this.PreviewPane.Focus(FocusState.Programmatic));
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

    /// <summary>
    /// Renders this window's own border - see <see cref="LayoutHelper.GetHostBoxStyle"/> -
    /// and assigns it to <see cref="BorderImage"/>. Unlike <see cref="PreviewPane"/>'s content,
    /// this is rendered directly by the host rather than the pane, since the border is
    /// deliberately not one of the pane's concerns.
    /// </summary>
    /// <returns>
    /// The window-region dimensions (see <see cref="ApplyWindowRegion"/>) that match the
    /// rendered border - deliberately *not* applied here, so the window's clip region only ever
    /// changes once <see cref="ShowWindowAsync"/> reveals fully-built content, not partway
    /// through rendering it. The caller passes these straight back into
    /// <see cref="ShowWindowAsync"/>.
    /// </returns>
    private async Task<(int Width, int Height, int CornerRadius)> RenderBorderAsync(PreviewLayout previewLayout, BoxStyle hostBoxStyle)
    {
        // render against a zero-based host box - a border image is its own bitmap, so its
        // pixel coordinates need to start at (0,0) regardless of where the (possibly
        // negative, once enlarged outward from a zero-based content box) host bounds would
        // otherwise place it.
        var localHostBounds = LayoutHelper.GetHostBounds(previewLayout.CanvasLayout.CanvasBounds.OuterBounds, hostBoxStyle)
            .MoveTo(new PointInfo(0, 0));

        using var borderBitmap = DrawingHelper.RenderBorder(localHostBounds, hostBoxStyle);

        await this.InvokeOnUiThreadAsync(
            () =>
            {
                var highDpiScalingRatio = this.GetHighDpiScalingRatio();
                this.BorderImage.Width = borderBitmap.Width * highDpiScalingRatio;
                this.BorderImage.Height = borderBitmap.Height * highDpiScalingRatio;
                this.BorderImage.Source = PreviewWindow.ToBitmapImage(borderBitmap);

                // position PreviewPane so it lines up exactly with the transparent hole in
                // the middle of the border image - the offset is always the host box's own
                // margin+border thickness, regardless of where localHostBounds itself sits.
                var offsetX = (localHostBounds.ContentBounds.X - localHostBounds.OuterBounds.X) * (decimal)highDpiScalingRatio;
                var offsetY = (localHostBounds.ContentBounds.Y - localHostBounds.OuterBounds.Y) * (decimal)highDpiScalingRatio;
                this.PreviewPane.Margin = new Thickness((double)offsetX, (double)offsetY, 0, 0);
            }).ConfigureAwait(false);

        return (borderBitmap.Width, borderBitmap.Height, (int)hostBoxStyle.BorderStyle.Left);
    }

    private async Task SetPreviewPaneLayoutAsync(PreviewLayout previewLayout, ScreenInfo activatedScreen)
    {
        await this.InvokeOnUiThreadAsync(
            () =>
            {
                this.PreviewPane.Layout = previewLayout;
                this.PreviewPane.ActiveScreen = activatedScreen;
            }).ConfigureAwait(false);
    }

    /// <summary>
    /// Copies <paramref name="bitmap"/>'s pixels directly into a <see cref="WriteableBitmap"/>,
    /// rather than round-tripping through a PNG encode (GDI+) + decode (WinUI). Works as a
    /// straight byte copy because <see cref="DrawingHelper.RenderBorder"/> always produces
    /// <see cref="PixelFormat.Format32bppPArgb"/>, which is byte-for-byte the same layout as
    /// <see cref="WriteableBitmap"/>'s own pixel buffer (BGRA8, premultiplied).
    /// </summary>
    private static WriteableBitmap ToBitmapImage(Bitmap bitmap)
    {
        var writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height);

        var bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        try
        {
            var byteCount = bitmapData.Stride * bitmapData.Height;
            var buffer = new byte[byteCount];
            Marshal.Copy(bitmapData.Scan0, buffer, 0, byteCount);

            using var pixelStream = writeableBitmap.PixelBuffer.AsStream();
            pixelStream.Write(buffer, 0, byteCount);
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        writeableBitmap.Invalidate();
        return writeableBitmap;
    }
}
