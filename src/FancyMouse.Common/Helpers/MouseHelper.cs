using FancyMouse.Models.Drawing;

namespace FancyMouse.Common.Helpers;

public static class MouseHelper
{
    /// <summary>
    /// Calculates where to move the cursor to by projecting a point from
    /// the preview image onto the desktop and using that as the target location.
    /// </summary>
    /// <remarks>
    /// The preview image origin is (0, 0) but the desktop origin may be non-zero,
    /// or even negative if the primary monitor is not the at the top-left of the
    /// entire desktop rectangle, so results may contain negative coordinates.
    /// </remarks>
    public static PointInfo GetJumpLocation(PointInfo previewLocation, SizeInfo previewSize, RectangleInfo desktopBounds)
    {
        return previewLocation
            .Scale(previewSize.ScaleToFitRatio(desktopBounds.Size))
            .Offset(desktopBounds.Location);
    }
}
