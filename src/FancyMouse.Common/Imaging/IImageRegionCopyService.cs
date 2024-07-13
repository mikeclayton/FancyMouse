using System.Drawing;
using FancyMouse.Common.Models.Drawing;

namespace FancyMouse.Common.Imaging;

public interface IImageRegionCopyService
{
    /// <summary>
    /// Copies the source region from the provider's source image (e.g. the interactive desktop,
    /// a static image, etc) to the target region on the specified Graphics object.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are used to capture regions of the interactive desktop
    /// during runtime, or to capture regions of a static reference image during unit tests.
    /// </remarks>
    void CopyImageRegion(
        Graphics targetGraphics,
        RectangleInfo sourceBounds,
        RectangleInfo targetBounds);
}
