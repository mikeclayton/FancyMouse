using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Styles;

public class PreviewStyle
{
    public sealed class Builder
    {
        public SizeInfo? CanvasSize
        {
            get;
            set;
        }

        public BoxStyle? CanvasStyle
        {
            get;
            set;
        }

        public BoxStyle? ScreenshotStyle
        {
            get;
            set;
        }

        public PreviewStyle Build()
        {
            return new PreviewStyle(
                canvasSize: this.CanvasSize ?? throw new InvalidOperationException($"{nameof(this.CanvasSize)} must be initialized before calling {nameof(this.Build)}."),
                canvasStyle: this.CanvasStyle ?? throw new InvalidOperationException($"{nameof(this.CanvasStyle)} must be initialized before calling {nameof(this.Build)}."),
                screenshotStyle: this.ScreenshotStyle ?? throw new InvalidOperationException($"{nameof(this.ScreenshotStyle)} must be initialized before calling {nameof(this.Build)}."));
        }
    }

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
