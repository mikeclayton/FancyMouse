using System.Drawing;
using FancyMouse.Common.Models.Drawing;

namespace FancyMouse.Common.Imaging;

/// <summary>
/// Implements an IImageRegionCopyService that uses the specified image as the copy source.
/// This is used for testing the DrawingHelper rather than as part of the main application.
/// </summary>
public sealed class StaticImageRegionCopyService : IImageRegionCopyService
{
    public StaticImageRegionCopyService(Image sourceImage)
    {
        this.SourceImage = sourceImage ?? throw new ArgumentNullException(nameof(sourceImage));
    }

    private Image SourceImage
    {
        get;
    }

    /// <summary>
    /// Copies the source region from the static source image
    /// to the target region on the specified Graphics object.
    /// </summary>
    public void CopyImageRegion(
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
