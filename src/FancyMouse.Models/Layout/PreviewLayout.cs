using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Layout;

/// <summary>
/// Defines everything the preview pane draws - its outer margin, outer border, background and
/// the canvas grid of devices and screens inside it - but not a position on the desktop. Every
/// bounds in <see cref="CanvasLayout"/> (and everything nested under it) is relative to the
/// pane's own top-left corner as the origin; deciding where that origin actually lands on the
/// desktop - centering on the activated location, clamping to the activated screen - is
/// entirely the hosting window's job (see <c>LayoutHelper.PositionOnScreen</c>).
/// </summary>
public sealed class PreviewLayout
{
    public sealed class Builder
    {
        /// <summary>
        /// Gets or sets the preview pane's overall size, margin included.
        /// </summary>
        public SizeInfo? PreviewSize
        {
            get;
            set;
        }

        public CanvasLayout.Builder? CanvasLayout
        {
            get;
            set;
        }

        public PreviewLayout Build()
        {
            return new PreviewLayout(
                previewSize: this.PreviewSize ?? throw new InvalidOperationException($"{nameof(this.PreviewSize)} must be initialized before calling {nameof(this.Build)}."),
                canvasLayout: (this.CanvasLayout ?? throw new InvalidOperationException($"{nameof(this.CanvasLayout)} must be initialized before calling {nameof(this.Build)}."))
                    .Build());
        }
    }

    public PreviewLayout(
        SizeInfo previewSize,
        CanvasLayout canvasLayout)
    {
        this.PreviewSize = previewSize ?? throw new ArgumentNullException(nameof(previewSize));
        this.CanvasLayout = canvasLayout ?? throw new ArgumentNullException(nameof(canvasLayout));
    }

    /// <summary>
    /// Gets the preview pane's overall size, margin included.
    /// </summary>
    public SizeInfo PreviewSize
    {
        get;
    }

    public CanvasLayout CanvasLayout
    {
        get;
    }
}
