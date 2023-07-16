using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Styles;

public class PreviewStyle
{
    public PreviewStyle(
        SizeInfo canvasSize,
        BoxStyle canvasStyle,
        BoxStyle screenshotStyle)
    {
        this.CanvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
        this.CanvasStyle = canvasStyle ?? throw new ArgumentNullException(nameof(canvasStyle));
        this.ScreenshotStyle = screenshotStyle ?? throw new ArgumentNullException(nameof(screenshotStyle));
    }

    public SizeInfo CanvasSize
    {
        get;
    }

    public BoxStyle CanvasStyle
    {
        get;
    }

    public BoxStyle ScreenshotStyle
    {
        get;
    }
}
