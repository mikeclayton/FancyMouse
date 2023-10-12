using FancyMouse.Models.Drawing;

namespace FancyMouse.Helpers.Screenshot;

internal interface IScreenshotProvider
{
    void DrawScreenshot(
        Graphics targetGraphics,
        RectangleInfo sourceBounds,
        RectangleInfo targetBounds);
}
