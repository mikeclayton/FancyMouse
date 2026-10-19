using FancyMouse.Drawing.Capture;
using FancyMouse.Models.Display;
using FancyMouse.Models.Layout;
using FancyMouse.WinUI3.Controls.Helpers;
using FancyMouse.WinUI3.Win32Gen;

using Microsoft.UI.Xaml;

using Windows.Win32.Foundation;

namespace FancyMouse.WinUI3.UI;

internal sealed partial class PreviewWindow
{
    /// <summary>
    /// Waits for the screenshot capture pipeline to complete all of the requested
    /// screenshot captures, and logs if any of them fail.
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

    private Task InvokeOnUiThreadAsync(Action action)
        => this.DispatcherQueue.InvokeAsync(action);

    /// <summary>
    /// Sets this window to be the operating system's foreground window.
    /// </summary>
    /// <remarks>
    /// This only works if the current process meets specific conditions (documented at
    /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow#remarks).
    /// Note that it generally *will* meet those conditions if called from the HotKeyManager's
    /// event handler because that satisfies the "calling process received the last input event."
    /// requirement so the process can set the foreground window even if it isn't currently
    /// the foreground process.
    /// </remarks>
    private void SetAsForegroundWindow()
    {
        var hWnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);
        _ = User32.SetForegroundWindow(hWnd)
            .IgnoreFailure();
    }

    /// <summary>
    /// Sets keyboard focus on <see cref="PreviewPane"/>, which owns all keyboard navigation
    /// (see <see cref="PreviewPane.NavigateTo"/>) via its own <c>PreviewKeyDown</c>.
    /// </summary>
    private void FocusPreviewPane()
    {
        if (this.PreviewPane.Focus(FocusState.Programmatic))
        {
            // success
            return;
        }

        // <control>.Focus can fail silently if the pane hasn't finished layout
        // yet - most likely the very first time the window is ever shown after
        // the app starts - so retry (once) on the next UI thread tick
        this.DispatcherQueue.TryEnqueue(() => this.PreviewPane.Focus(FocusState.Programmatic));
    }

    /// <summary>
    /// Gets the window-region bounds (see <see cref="ApplyRoundRectRegion"/>) that match the
    /// preview pane's own outer border - the pane fills this window exactly (see
    /// PreviewWindow.xaml), so the region is the pane's border box (inset by its margin, which
    /// is clipped away), with its corners rounded to match the border's thickness.
    /// </summary>
    /// <remarks>
    /// Deliberately *not* applied here, so the window's clip region only ever changes once
    /// <see cref="ShowWindowAsync"/> reveals fully-built content, not partway through building
    /// it. The caller passes these straight into <see cref="ShowWindowAsync"/>.
    /// </remarks>
    private static (int X, int Y, int Width, int Height, int CornerRadius) GetWindowRegion(PreviewLayout previewLayout)
    {
        var canvasLayout = previewLayout.CanvasLayout;
        var borderBounds = canvasLayout.CanvasBounds.BorderBounds;
        return ((int)borderBounds.X, (int)borderBounds.Y, (int)borderBounds.Width, (int)borderBounds.Height, (int)canvasLayout.CanvasStyle.BorderStyle.Left);
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
}
