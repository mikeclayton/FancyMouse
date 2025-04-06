using System.Drawing.Drawing2D;
using FancyMouse.Common.Helpers;
using FancyMouse.Common.Imaging;
using FancyMouse.Common.Models.Drawing;

namespace FancyMouse.Internal.Imaging;

/// <summary>
/// Implements an IImageRegionCopyService that uses the specified image as the copy source.
/// The whole thumbnail image is used as the source regardless of the specified source coordinates.
/// </summary>
public sealed class RemoteImageRegionCopyService : IImageRegionCopyService
{
    public RemoteImageRegionCopyService(MwbApiClient mwbApiClient)
    {
        this.MwbApiClient = mwbApiClient ?? throw new ArgumentNullException(nameof(mwbApiClient));
    }

    private MwbApiClient MwbApiClient
    {
        get;
    }

    /// <summary>
    /// Copies the entire thumbnail from the static source image
    /// to the target region on the specified Graphics object.
    /// </summary>
    public void CopyImageRegion(
        Graphics targetGraphics,
        RectangleInfo sourceBounds,
        RectangleInfo targetBounds)
    {
        using var remoteThumbnail = this.MwbApiClient.GetRemoteThumbnail(
            screenId: 0,
            sourceX: (int)sourceBounds.X,
            sourceY: (int)sourceBounds.Y,
            sourceWidth: (int)sourceBounds.Width,
            sourceHeight: (int)sourceBounds.Height,
            targetWidth: (int)targetBounds.Width,
            targetHeight: (int)targetBounds.Height,
            cancellationToken: default).ConfigureAwait(false).GetAwaiter().GetResult();

        // prevent the background bleeding through into screen images
        // (see https://github.com/mikeclayton/FancyMouse/issues/44)
        targetGraphics.PixelOffsetMode = PixelOffsetMode.Half;
        targetGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;

        targetGraphics.DrawImage(
            image: remoteThumbnail,
            destRect: targetBounds.ToRectangle(),
            srcRect: new(0, 0, remoteThumbnail.Width, remoteThumbnail.Height),
            srcUnit: GraphicsUnit.Pixel);
    }
}
