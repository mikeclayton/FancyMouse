using System.Drawing;
using System.Drawing.Imaging;

using FancyMouse.Common.Helpers;
using FancyMouse.Common.Imaging;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;
using FancyMouse.WinUI3.Internal.Helpers;
using FancyMouse.WinUI3.Win32Gen;

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;

using Windows.Graphics;
using Windows.System;
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
        this.RootGrid.PreviewKeyDown += this.PreviewWindow_PreviewKeyDown;
        this.PreviewPane.PreviewKeyDown += this.PreviewWindow_PreviewKeyDown;
        this.PreviewPane.ScreenshotClicked += this.PreviewPane_ScreenshotClicked;
    }

    private void PreviewWindow_Activated(object sender, WindowActivatedEventArgs e)
    {
        switch (e.WindowActivationState)
        {
            case WindowActivationState.CodeActivated:
                this.PreviewPane.Focus(FocusState.Programmatic);
                break;
            case WindowActivationState.Deactivated:
                this.HideWindow();
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private void PreviewWindow_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Escape)
        {
            this.HideWindow();
            return;
        }

        var screens = ScreenHelper.GetAllScreens().ToList();
        if (screens.Count == 0)
        {
            return;
        }

        var currentLocation = MouseHelper.GetCursorPosition();
        var currentScreen = ScreenHelper.GetScreenFromPoint(screens, currentLocation);
        var currentScreenIndex = screens.IndexOf(currentScreen);
        var targetScreen = default(ScreenInfo?);

        switch (e.Key)
        {
            case >= VirtualKey.Number1 and <= VirtualKey.Number9:
                {
                    // number keys 1-9 - move to the numbered screen
                    var screenNumber = e.Key - VirtualKey.Number0;
                    /* note - screen *numbers* are 1-based, screen *indexes* are 0-based */
                    targetScreen = (screenNumber <= screens.Count)
                        ? targetScreen = screens[screenNumber - 1]
                        : null;
                    break;
                }

            case >= VirtualKey.NumberPad1 and <= VirtualKey.NumberPad9:
                {
                    // numpad keys 1-9 - move to the numbered screen
                    var screenNumber = e.Key - VirtualKey.NumberPad0;
                    /* note - screen *numbers* are 1-based, screen *indexes* are 0-based */
                    targetScreen = (screenNumber <= screens.Count)
                        ? targetScreen = screens[screenNumber - 1]
                        : null;
                    break;
                }

            case VirtualKey.P:
                // "P" - move to the primary screen
                targetScreen = screens.Single(screen => screen.Primary);
                break;
            case VirtualKey.Left:
                // move to the previous screen, looping back to the end if needed
                var prevIndex = (currentScreenIndex - 1 + screens.Count) % screens.Count;
                targetScreen = screens[prevIndex];
                break;
            case VirtualKey.Right:
                // move to the next screen, looping round to the start if needed
                var nextIndex = (currentScreenIndex + 1) % screens.Count;
                targetScreen = screens[nextIndex];
                break;
            case VirtualKey.Home:
                // move to the first screen
                targetScreen = screens.First();
                break;
            case VirtualKey.End:
                // move to the last screen
                targetScreen = screens.Last();
                break;
        }

        if (targetScreen is not null)
        {
            MouseHelper.SetCursorPosition(targetScreen.DisplayArea.Midpoint);
            this.HideWindow();
        }
    }

    private void PreviewPane_ScreenshotClicked(object? sender, ScreenshotClickedEventArgs e)
    {
        var logger = this.Logger;

        logger.Info(string.Join(
            '\n',
            "-----------",
            nameof(PreviewWindow.PreviewPane_ScreenshotClicked),
            "-----------",
            $"clicked location = {e.Location}"));

        MouseHelper.SetCursorPosition(e.Location);
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

        // hide the form while we redraw it...
        await this.HideWindowAsync()
            .ConfigureAwait(false);

        // capture this first so we get an accurate current mouse location
        // (in case the user moves it a few pixels while the form is rendered)
        var activatedLocation = MouseHelper.GetCursorPosition();

        var appSettings = ConfigHelper.AppSettings ?? throw new InvalidOperationException();

        var displayInfo = DeviceHelper.GetDisplayInfo();

        var activatedScreen = DeviceHelper.GetActivatedScreen(displayInfo.Devices[0], activatedLocation);

        var previewStyle = appSettings.PreviewStyle;
        var previewLayout = LayoutHelper.GetPreviewLayout(
            previewStyle,
            displayInfo,
            activatedScreen: activatedScreen);

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

        await this.RenderBorderAsync(previewLayout, hostBoxStyle)
            .ConfigureAwait(false);

        // builds every screen's bezel + placeholder fill immediately, so there's something to
        // show as soon as the window becomes visible - screenshots backfill afterwards
        await this.SetPreviewPaneLayoutAsync(previewLayout)
            .ConfigureAwait(false);

        // one capture provider per device - see IScreenshotCaptureProvider/
        // DesktopScreenshotCaptureProvider remarks for why a single instance is safe (and
        // necessary) to share across all of that device's screens
        var captureProviders = displayInfo.Devices
            .Select(deviceInfo => (IScreenshotCaptureProvider)new DesktopScreenshotCaptureProvider())
            .ToList();

        // kick off a capture request per screen without awaiting them yet, so they can all
        // start running (subject to each provider's own parallel-vs-series capabilities)
        var captureRequests = previewLayout.CanvasLayout.DeviceLayouts
            .SelectMany((deviceLayout, deviceIndex) => deviceLayout.ScreenLayouts.Select(
                screenLayout => new ScreenCaptureRequest(
                    screenLayout,
                    captureProviders[deviceIndex].CaptureAsync(
                        screenLayout.ScreenInfo.DisplayArea,
                        screenLayout.ScreenBounds.ContentBounds.Size))))
            .ToList();

        // the activated screen's own capture must complete - and be applied - before the
        // window is shown, otherwise a *later* capture of that screen would risk capturing
        // the preview window itself, since it's positioned on top of the activated screen
        var activatedRequest = captureRequests.Single(
            request => object.ReferenceEquals(request.ScreenLayout.ScreenInfo, activatedScreen));
        await this.ApplyScreenshotAsync(activatedRequest)
            .ConfigureAwait(false);

        await this.ShowWindowAsync()
            .ConfigureAwait(false);

        // backfill the remaining screens in the background, in whatever order their captures
        // actually complete - "slow" sources (e.g. a future remote capture provider) shouldn't
        // hold up screens that are already done
        var remainingRequests = captureRequests
            .Where(request => !object.ReferenceEquals(request, activatedRequest))
            .ToList();
        _ = this.BackfillScreenshotsAsync(remainingRequests, captureProviders);
    }

    /// <summary>
    /// Awaits the remaining screen captures in completion order (not list order) and applies
    /// each one as soon as it's ready, then disposes <paramref name="captureProviders"/> once
    /// every request for this activation - including the activated screen's own, already
    /// applied before this was called - has finished with them.
    /// </summary>
    private async Task BackfillScreenshotsAsync(
        List<ScreenCaptureRequest> requests, List<IScreenshotCaptureProvider> captureProviders)
    {
        try
        {
            var pending = requests.ToList();
            while (pending.Count > 0)
            {
                var completedTask = await Task.WhenAny(pending.Select(request => request.CaptureTask))
                    .ConfigureAwait(false);
                var completedRequest = pending.Single(request => request.CaptureTask == completedTask);
                pending.Remove(completedRequest);
                await this.ApplyScreenshotAsync(completedRequest)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            foreach (var provider in captureProviders.OfType<IDisposable>())
            {
                provider.Dispose();
            }
        }
    }

    private async Task ApplyScreenshotAsync(ScreenCaptureRequest request)
    {
        using var image = await request.CaptureTask.ConfigureAwait(false);
        await this.InvokeOnUiThreadAsync(
                () => this.PreviewPane.SetScreenshot(request.ScreenLayout, image))
            .ConfigureAwait(false);
    }

    private void ClearPreview()
    {
        if ((this.BorderImage.Source is null) && (this.PreviewPane.Layout is null))
        {
            return;
        }

        this.BorderImage.Source = null;
        this.PreviewPane.Layout = null;

        // force preview image memory to be released - otherwise
        // all the disposed images can pile up without being GC'ed
        GC.Collect();
        GC.WaitForPendingFinalizers();
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
                this.PreviewPane.Focus(FocusState.Programmatic);
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

    private async Task SetPreviewPaneLayoutAsync(PreviewLayout previewLayout)
    {
        await this.InvokeOnUiThreadAsync(
            () =>
            {
                this.PreviewPane.Layout = previewLayout;
            }).ConfigureAwait(false);
    }

    private static BitmapImage ToBitmapImage(Bitmap bitmap)
    {
        var bitmapImage = new BitmapImage();
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        bitmapImage.SetSource(stream.AsRandomAccessStream());
        return bitmapImage;
    }

    /// <summary>
    /// Pairs a screen with its in-flight capture request, so a result can be routed back to
    /// the right <see cref="PreviewPane"/> slot once it completes (see
    /// <see cref="PreviewPane.SetScreenshot"/>).
    /// </summary>
    private sealed record ScreenCaptureRequest(ScreenLayout ScreenLayout, Task<Bitmap> CaptureTask);
}
