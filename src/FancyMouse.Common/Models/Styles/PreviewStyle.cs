using FancyMouse.Common.Models.Drawing;

namespace FancyMouse.Common.Models.Styles;

public sealed class PreviewStyle
{
    public PreviewStyle(
        SizeInfo canvasSize,
        BoxStyle canvasStyle,
        BoxStyle screenStyle)
    {
        this.CanvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
        this.CanvasStyle = canvasStyle ?? throw new ArgumentNullException(nameof(canvasStyle));
        this.ScreenStyle = screenStyle ?? throw new ArgumentNullException(nameof(screenStyle));
    }

    public SizeInfo CanvasSize
    {
        get;
    }

    public BoxStyle CanvasStyle
    {
        get;
    }

    public BoxStyle ScreenStyle
    {
        get;
    }
}
