using FancyMouse.Drawing.Helpers;
using FancyMouse.Models.Drawing;

using SkiaSharp;

namespace FancyMouse.Drawing.Capture;

/// <summary>
/// Implements an <see cref="IScreenshotCaptureProvider"/> that captures from a fixed source
/// image instead of the interactive desktop. This is used for testing <see cref="Helpers.DrawingHelper"/>
/// and the preview capture pipeline rather than as part of the main application.
/// </summary>
public sealed class StaticScreenshotCaptureProvider : IScreenshotCaptureProvider
{
    public StaticScreenshotCaptureProvider(SKBitmap sourceImage)
    {
        this.SourceImage = sourceImage ?? throw new ArgumentNullException(nameof(sourceImage));
    }

    private SKBitmap SourceImage
    {
        get;
    }

    public Task<SKBitmap> CaptureAsync(
        RectangleInfo sourceArea,
        SizeInfo thumbnailSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = thumbnailSize.Round().ToSize();
        var thumbnailImage = SkiaHelper.CreateBitmap(target.Width, target.Height, SKAlphaType.Premul);
        using (var thumbnailCanvas = new SKCanvas(thumbnailImage))
        using (var sourceImage = SkiaHelper.AsImage(this.SourceImage))
        {
            // prevent the background bleeding through into screen images by sampling
            // the nearest source pixel rather than blending neighbouring ones
            // (see https://github.com/mikeclayton/FancyMouse/issues/44)
            thumbnailCanvas.DrawImage(
                sourceImage,
                sourceArea.ToRectangle().ToSKRect(),
                SKRect.Create(0, 0, target.Width, target.Height),
                new SKSamplingOptions(SKFilterMode.Nearest));
        }

        return Task.FromResult(thumbnailImage);
    }
}
