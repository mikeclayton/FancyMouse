using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Layout;

internal sealed class PreviewLayout
{
    public PreviewLayout(
        SizeInfo previewSize,
        RectangleInfo borderCoords,
        Region backgroundRegion)
    {
        this.PreviewSize = previewSize ?? throw new ArgumentNullException(nameof(previewSize));
    }

    public SizeInfo PreviewSize
    {
        get;
    }
}
