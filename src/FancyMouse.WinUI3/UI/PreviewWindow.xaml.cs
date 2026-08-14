using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

using FancyMouse.Common.Capture;
using FancyMouse.Common.Helpers;
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
    /// <see cref="ScreenshotCapturePipeline"/> - cancelled (and replaced) whenever the preview
    /// is cleared, whether that's because a new activation superseded this one or because the
    /// window was simply hidden (see <see cref="ClearPreview"/>), so outstanding captures for a
    /// no-longer-relevant activation don't keep doing GDI work in the background.
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
    /// Initializes some settings on the application window.
    /// </summary>
    private void InitializeWindow()
    {
        var appWindow = this.AppWindow;
        var presenter = appWindow.Presenter as OverlappedPresenter;
        if (presenter != null)
        {
            var hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);

            // get the current window style
            var result = User32.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE)
                .ThrowIfFailed()
                .GetValue();

            // set the window to be borderless, with no title bar, and hide all of the max / min / close buttons
            var style = (WINDOW_STYLE)result;
            style &= ~WINDOW_STYLE.WS_OVERLAPPEDWINDOW;
            style |= WINDOW_STYLE.WS_POPUP;
            _ = User32.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style)
                .ThrowIfFailed();

            // get the current extended window style
            result = User32.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE)
                .ThrowIfFailed()
                .GetValue();

            // set the window to be borderless, with no title bar, and hide all of the max / min / close buttons
            var exStyle = (WINDOW_EX_STYLE)result;
            exStyle |= WINDOW_EX_STYLE.WS_EX_TOOLWINDOW; // hide the taskbar icon
            exStyle |= WINDOW_EX_STYLE.WS_EX_TOPMOST;    // make topmost
            _ = User32.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)exStyle)
                .ThrowIfFailed();
        }

        this.Activated += this.PreviewWindow_Activated;
        this.PreviewPane.NavigateTo += this.PreviewPane_NavigateTo;
        this.PreviewPane.Cancel += this.PreviewPane_Cancel;
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

    public async Task ShowPreviewAsync()
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

        ////// diagnostic timing - not wired up live, but kept here as a ready-made way to see
        ////// where activation time goes if that's ever worth investigating again. Uncomment
        ////// (along with every DiagCheckpoint(...) call below) when needed; don't ship it live.
        ////var diagStopwatch = Stopwatch.StartNew();
        ////void DiagCheckpoint(string label)
        ////    => logger.Info($"DIAG checkpoint {label}: elapsed={diagStopwatch.ElapsedMilliseconds}ms");

        // hide the form while we redraw it...
        await this.HideWindowAsync()
            .ConfigureAwait(false);
        ////DiagCheckpoint("HideWindowAsync done");

        // capture this first so we get an accurate current mouse location
        // (in case the user moves it a few pixels while the form is rendered)
        var activatedLocation = MouseHelper.GetCursorPosition();

        var appSettings = ConfigHelper.AppSettings ?? throw new InvalidOperationException();

        var displayInfo = DeviceHelper.GetDisplayInfo();
        ////DiagCheckpoint("GetDisplayInfo done");

        var activatedScreen = DeviceHelper.GetActivatedScreen(displayInfo.Devices[0], activatedLocation);

        var previewStyle = appSettings.PreviewStyle;
        var previewLayout = LayoutHelper.GetPreviewLayout(
            previewStyle,
            displayInfo,
            activatedScreen: activatedScreen);
        ////DiagCheckpoint("GetPreviewLayout done");

        // the outer border is this window's own responsibility, not the preview pane's -
        // see LayoutHelper.GetHostBoxStyle. PreviewLayout itself has no desktop position
        // (only a size - see PreviewLayout), so positioning the window on the desktop -
        // centered on the activated location, clamped to the activated screen - is entirely
        // this window's own job too.
        var hostBoxStyle = LayoutHelper.GetHostBoxStyle(previewStyle.CanvasStyle);
        var hostBounds = LayoutHelper.GetHostBounds(new RectangleInfo(previewLayout.PreviewSize), hostBoxStyle);
        var positionedHostOuterBounds = LayoutHelper.PositionOnScreen(hostBounds.OuterBounds, activatedScreen, activatedLocation);

        await this.PositionWindowAsync(positionedHostOuterBounds)
            .ConfigureAwait(false);
        ////DiagCheckpoint("PositionWindowAsync done");

        await this.RenderBorderAsync(previewLayout, hostBoxStyle)
            .ConfigureAwait(false);
        ////DiagCheckpoint("RenderBorderAsync done");

        // builds every screen's bezel + placeholder fill immediately, so there's something to
        // show as soon as the window becomes visible - screenshots backfill afterwards
        await this.SetPreviewPaneLayoutAsync(previewLayout, activatedScreen)
            .ConfigureAwait(false);
        ////DiagCheckpoint("SetPreviewPaneLayoutAsync done");

        // cancel whatever the previous activation - if any - might still have running, and
        // start a fresh cancellation scope for this one (see ClearPreview and the
        // activationCancellation remarks)
        this.activationCancellation?.Cancel();
        this.activationCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        this.activationCancellation = cancellation;

        var pipeline = new ScreenshotCapturePipeline(new PreviewPaneScreenshotSink(this), cancellation.Token);

        // one capture provider per device - see IScreenshotCaptureProvider/
        // DesktopScreenshotCaptureProvider remarks for why a single instance is safe (and
        // necessary) to share across all of that device's screens. Ownership of each provider
        // transfers to the pipeline - see ScreenshotCapturePipeline.DisposeAsync.
        var captureTasks = new List<(ScreenLayout ScreenLayout, Task<Bitmap> CaptureTask)>();
        foreach (var deviceLayout in previewLayout.CanvasLayout.DeviceLayouts)
        {
            captureTasks.AddRange(pipeline.AddCaptureTasks(
                deviceLayout, new DesktopScreenshotCaptureProvider()));
        }

        ////DiagCheckpoint("all capture requests kicked off");

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
            await Task.WhenAny(Task.WhenAll(allCaptureTasks), Task.Delay(PreviewWindow.ScreenshotGracePeriod, cancellation.Token))
                .ConfigureAwait(false);
            ////DiagCheckpoint("grace period race done");
            await activatedCaptureTask
                .ConfigureAwait(false);
            ////DiagCheckpoint("activated screen capture done");

            await this.ShowWindowAsync()
                .ConfigureAwait(false);
            ////DiagCheckpoint("ShowWindowAsync done - window visible");
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
        // (background, bezels, content) - left to the GC's own schedule, those pile up as
        // uncollected garbage quickly enough under repeat activation to show up as inflated
        // memory usage in Task Manager, even though most of it is just waiting to be freed.
        // A background (non-blocking) collection reclaims it without stalling this thread the
        // way a blocking GC.Collect() + WaitForPendingFinalizers() would.
        GC.Collect(2, GCCollectionMode.Optimized, blocking: false);
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

    private async Task InvokeOnUiThreadAsync(Action action)
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
        this.AppWindow.Hide();
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

    private async Task ShowWindowAsync()
    {
        await this.InvokeOnUiThreadAsync(
            () =>
            {
                var presenter = this.AppWindow.Presenter as OverlappedPresenter
                    ?? throw new InvalidOperationException();

                if (!this.Visible)
                {
                    // we seem to need to turn off topmost and then re-enable it again
                    // when we show the form - otherwise it doesn't always get shown topmost...
                    presenter.IsAlwaysOnTop = false;
                    presenter.IsAlwaysOnTop = true;
                }

                this.AppWindow.Show();

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
    /// and assigns it to <see cref="BorderImage"/>. Unlike <see cref="PreviewPane"/>'s
    /// content, this is rendered directly by the host rather than the pane, since the border
    /// is deliberately not one of the pane's concerns.
    /// </summary>
    private async Task RenderBorderAsync(PreviewLayout previewLayout, BoxStyle hostBoxStyle)
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

    /// <summary>
    /// Adapts <see cref="PreviewPane.SetScreenshot"/> to <see cref="IScreenshotCaptureSink"/> -
    /// marshals onto the UI thread, since <see cref="ScreenshotCapturePipeline"/> pushes results
    /// as soon as they're captured, which generally isn't the UI thread.
    /// </summary>
    private sealed class PreviewPaneScreenshotSink : IScreenshotCaptureSink
    {
        public PreviewPaneScreenshotSink(PreviewWindow window)
        {
            this.Window = window;
        }

        private PreviewWindow Window
        {
            get;
        }

        public Task SetScreenshotAsync(ScreenLayout screenLayout, Bitmap bitmap)
            => this.Window.InvokeOnUiThreadAsync(() => this.Window.PreviewPane.SetScreenshot(screenLayout, bitmap));
    }
}
