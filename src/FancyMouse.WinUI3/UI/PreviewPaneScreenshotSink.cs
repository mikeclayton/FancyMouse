using System.Drawing;

using FancyMouse.Common.Capture;
using FancyMouse.Models.Layout;

namespace FancyMouse.WinUI3.UI;

/// <summary>
/// Adapts <see cref="PreviewPane.SetScreenshot"/> to <see cref="IScreenshotCaptureSink"/> -
/// marshals onto the UI thread, since <see cref="ScreenshotCapturePipeline"/> pushes results
/// as soon as they're captured, which generally isn't the UI thread.
/// </summary>
internal sealed class PreviewPaneScreenshotSink : IScreenshotCaptureSink
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
