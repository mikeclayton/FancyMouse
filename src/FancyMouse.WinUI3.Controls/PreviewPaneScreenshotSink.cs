using System.Drawing;

using FancyMouse.Drawing.Capture;
using FancyMouse.Models.Layout;
using FancyMouse.WinUI3.Controls.Helpers;

namespace FancyMouse.WinUI3.Controls;

/// <summary>
/// Adapts <see cref="PreviewPane.SetScreenshot"/> to <see cref="IScreenshotCaptureSink"/> -
/// marshals onto the pane's own UI thread, since <see cref="ScreenshotCapturePipeline"/> pushes
/// results as soon as they're captured, which generally isn't the UI thread.
/// </summary>
public sealed class PreviewPaneScreenshotSink : IScreenshotCaptureSink
{
    public PreviewPaneScreenshotSink(PreviewPane previewPane)
    {
        this.PreviewPane = previewPane ?? throw new ArgumentNullException(nameof(previewPane));
    }

    private PreviewPane PreviewPane
    {
        get;
    }

    public Task SetScreenshotAsync(ScreenLayout screenLayout, Bitmap bitmap)
        => this.PreviewPane.DispatcherQueue.InvokeAsync(() => this.PreviewPane.SetScreenshot(screenLayout, bitmap));
}
