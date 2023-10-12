using FancyMouse.Models.Drawing;

namespace FancyMouse.Helpers.Screenshot;

/// <summary>
/// Implements an IScreenshotProvider that uses the specified image as the screenshot source.
/// (This is used for testing the DrawingHelper, rather than as part of the main application).
/// </summary>
internal sealed class ImageScreenshotProvider : IScreenshotProvider
{
    public ImageScreenshotProvider(Image sourceImage)
    {
        this.SourceImage = sourceImage ?? throw new ArgumentNullException(nameof(sourceImage));
    }

    private Image SourceImage
    {
        get;
    }

    /// <summary>
    /// Draws a screen capture from the current desktop window onto the image
    /// wrapped by the specified Graphics object.
    /// </summary>
    public void DrawScreenshot(
        Graphics targetGraphics,
        RectangleInfo sourceBounds,
        RectangleInfo targetBounds)
    {
        targetGraphics.DrawImage(
            image: this.SourceImage,
            destRect: targetBounds.ToRectangle(),
            srcRect: sourceBounds.ToRectangle(),
            srcUnit: GraphicsUnit.Pixel);
    }
}
