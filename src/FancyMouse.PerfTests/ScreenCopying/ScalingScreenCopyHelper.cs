using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using FancyMouse.ScreenCopying;

namespace FancyMouse.PerfTests.ScreenCopying;

public sealed class ScalingScreenCopyHelper : ICopyFromScreen
{
    public Bitmap CopyFromScreen(
        Rectangle desktopBounds, IEnumerable<Rectangle> desktopRegions, Size screenshotSize)
    {
        using var screenshot = new DefaultScreenCopyHelper().CopyFromScreen(
            desktopBounds, desktopRegions, screenshotSize);
        return ScalingScreenCopyHelper.ResizeImage(
            screenshot, screenshotSize);
    }

    private static Bitmap ResizeImage(Image image, Size size)
    {
        return ScalingScreenCopyHelper.ResizeImage(image, size.Width, size.Height);
    }

    /// <summary>
    /// Resize the image to the specified width and height.
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="width">The width to resize to.</param>
    /// <param name="height">The height to resize to.</param>
    /// <returns>The resized image.</returns>
    /// <remarks>
    /// See https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp/24199315#24199315
    /// </remarks>
    private static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);

        var destImage = new Bitmap(width, height);
        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            // low quality / fast
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.Low;
            graphics.SmoothingMode = SmoothingMode.HighSpeed;
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        }

        return destImage;
    }
}
