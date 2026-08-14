using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FancyMouse.Common.Helpers;

/// <summary>
/// Cheap blur approximation used to soften a screen's last real screenshot for use as a stand-in
/// placeholder while a fresh one is still loading - scales the source image down and back up
/// through GDI+'s own bilinear interpolation, rather than a true (and much more expensive)
/// Gaussian/box blur convolution. Good enough to read as "blurred" at thumbnail sizes without
/// meaningfully affecting capture-pipeline latency.
/// </summary>
public static class BlurHelper
{
    /// <summary>
    /// Returns a new bitmap, the same size as <paramref name="source"/>, blurred by scaling it
    /// down to <paramref name="intensity"/> of its original size and back up again.
    /// <paramref name="intensity"/> is the knob to experiment with - closer to 1.0 is barely
    /// blurred, closer to 0.0 is heavily blurred (and slightly cheaper, since there are fewer
    /// pixels to scale back up from).
    /// </summary>
    public static Bitmap CreateBlurredCopy(Bitmap source, decimal intensity)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (intensity is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(intensity), intensity, "Value must be greater than 0 and less than or equal to 1.");
        }

        var smallWidth = Math.Max(1, (int)(source.Width * intensity));
        var smallHeight = Math.Max(1, (int)(source.Height * intensity));

        using var smallImage = new Bitmap(smallWidth, smallHeight, PixelFormat.Format32bppPArgb);
        using (var smallGraphics = Graphics.FromImage(smallImage))
        {
            smallGraphics.InterpolationMode = InterpolationMode.Bilinear;
            smallGraphics.DrawImage(source, 0, 0, smallWidth, smallHeight);
        }

        var blurredImage = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb);
        using (var blurredGraphics = Graphics.FromImage(blurredImage))
        {
            blurredGraphics.InterpolationMode = InterpolationMode.Bilinear;
            blurredGraphics.DrawImage(smallImage, 0, 0, source.Width, source.Height);
        }

        return blurredImage;
    }
}
