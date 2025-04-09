using System.Collections.ObjectModel;
using System.Drawing;

using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Styles;

public sealed class PreviewStyle
{
    public PreviewStyle(
        SizeInfo canvasSize,
        BoxStyle canvasStyle,
        BoxStyle screenStyle,
        IEnumerable<Color> mwbColors)
    {
        this.CanvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
        this.CanvasStyle = canvasStyle ?? throw new ArgumentNullException(nameof(canvasStyle));
        this.ScreenStyle = screenStyle ?? throw new ArgumentNullException(nameof(screenStyle));
        this.MwbColors = new(
            (mwbColors ?? throw new ArgumentNullException(nameof(mwbColors)))
                .ToList());
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

    public ReadOnlyCollection<Color> MwbColors
    {
        get;
    }
}
