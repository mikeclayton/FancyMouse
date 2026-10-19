using FancyMouse.Drawing.Helpers;

using SkiaSharp;

namespace FancyMouse.Drawing.Blurring;

/// <summary>
/// Helpers to apply a blur to an image and return the result as a new image.
/// </summary>
/// <remarks>
/// If a screenshot capture takes too long during an activation the preview
/// window gets displayed using a blurred version of the previous screenshot
/// for that screen. When the new capture is complete the blurred image is
/// replaced with the actual screenshot - this creates the impression that
/// the preview window actually loads faster than it does.
///
/// The blur algorithm runs a three-pass separable box blur (each pass a
/// horizontal then a vertical sliding-window average) directly on the bitmap's
/// pixels - three passes closely approximates a true Gaussian blur, without
/// the cost of a real Gaussian convolution kernel, and each pass is O(pixel count)
/// regardless of blur radius. This is not specifically the *cheapest* possible
/// approximation: unlike the capture pipeline, blurred images are generated
/// in the background once the previous activation has been closed so the
/// blur process is not as performance-sensitive as the capture process.
/// </remarks>
internal static class BlurHelper
{
    /// <summary>
    /// Blur radius, in pixels, at the blurriest end of <see cref="CreateBlurredCopy"/>'s
    /// <c>blurIntensity</c> range (<c>blurIntensity</c> approaching 0). The radius scales down to 1px as
    /// <c>blurIntensity</c> approaches 1.
    /// </summary>
    private const int MaxBlurRadius = 40;

    /// <summary>
    /// Number of box-blur passes - see the class remarks for why three is the sweet spot between
    /// looking like a true Gaussian blur and not bothering with one.
    /// </summary>
    private const int BoxBlurPasses = 3;

    /// <summary>
    /// Returns a new bitmap, the same size as <paramref name="source"/>, desaturated and
    /// darkened, then blurred.
    /// </summary>
    public static SKBitmap CreateBlurredCopy(SKBitmap source, decimal blurIntensity, double saturation = 1.0, double brightness = 1.0)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (blurIntensity is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(blurIntensity), blurIntensity, "Value must be greater than 0 and less than or equal to 1.");
        }

        var mutedImage = SkiaHelper.CreateBitmap(source.Width, source.Height, SKAlphaType.Premul);
        try
        {
            using (var canvas = new SKCanvas(mutedImage))
            using (var colorFilter = SKColorFilter.CreateColorMatrix(BlurHelper.GetMuteColorMatrix(saturation, brightness)))
            using (var mutePaint = new SKPaint { ColorFilter = colorFilter })
            {
                canvas.DrawBitmap(source, 0, 0, mutePaint);
            }

            var radius = Math.Max(1, (int)((1 - blurIntensity) * BlurHelper.MaxBlurRadius));
            BlurHelper.ApplyBoxBlur(mutedImage, radius);

            return mutedImage;
        }
        catch
        {
            // the caller never receives mutedImage if we throw, so it can't dispose it
            mutedImage.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Builds a colour matrix that desaturates towards Rec. 601 luma weights by
    /// <paramref name="saturation"/> (0 = fully grey, 1 = unchanged), then scales the result by
    /// <paramref name="brightness"/> (0 = black, 1 = unchanged).
    /// </summary>
    private static float[] GetMuteColorMatrix(double saturation, double brightness)
    {
        const double lumaR = 0.30;
        const double lumaG = 0.59;
        const double lumaB = 0.11;

        // skia applies a colour matrix the usual "output row built from input columns" way
        // (output = matrix x input-as-a-column-vector) - the transpose of GDI+'s ColorMatrix
        // convention - so a matrix row is one *output* channel and a column is one *input*
        // channel's contribution to it. cell(input, output) below is written in terms of the
        // input channel: it contributes its own luma weight to every output channel (the
        // desaturated part), plus its own identity weight to its matching output channel
        // (the unmuted part).
        double Cell(int input, int output, double inputLuma)
            => brightness * (((1 - saturation) * inputLuma) + (saturation * (input == output ? 1 : 0)));

        // a 4x5 matrix, row by row - R, G, B and A outputs, each from (R, G, B, A, translation)
        // inputs - with alpha left unchanged and no translation
        return
        [
            (float)Cell(0, 0, lumaR), (float)Cell(1, 0, lumaG), (float)Cell(2, 0, lumaB), 0f, 0f,
            (float)Cell(0, 1, lumaR), (float)Cell(1, 1, lumaG), (float)Cell(2, 1, lumaB), 0f, 0f,
            (float)Cell(0, 2, lumaR), (float)Cell(1, 2, lumaG), (float)Cell(2, 2, lumaB), 0f, 0f,
            0f, 0f, 0f, 1f, 0f,
        ];
    }

    /// <summary>
    /// Runs <see cref="BoxBlurPasses"/> rounds of (horizontal then vertical) box blur directly
    /// over <paramref name="bitmap"/>'s own pixels, each round with the same <paramref name="radius"/>.
    /// </summary>
    private static unsafe void ApplyBoxBlur(SKBitmap bitmap, int radius)
    {
        try
        {
            var scan0 = (byte*)bitmap.GetPixels();
            var lineBuffer = new byte[Math.Max(bitmap.Width, bitmap.Height) * 4];

            for (var pass = 0; pass < BlurHelper.BoxBlurPasses; pass++)
            {
                BlurHelper.BoxBlurHorizontal(scan0, bitmap.Width, bitmap.Height, bitmap.RowBytes, radius, lineBuffer);
                BlurHelper.BoxBlurVertical(scan0, bitmap.Width, bitmap.Height, bitmap.RowBytes, radius, lineBuffer);
            }
        }
        finally
        {
            // the pixels were written directly through a pointer, so tell skia they've
            // changed in case it's cached anything derived from them
            bitmap.NotifyPixelsChanged();
        }
    }

    /// <summary>
    /// Averages each row's pixels over a sliding <paramref name="radius"/>-pixel window (edges
    /// clamped, i.e. the window past the edge repeats the edge pixel) - an O(width) pass per row
    /// regardless of <paramref name="radius"/>, since the window sum is updated incrementally
    /// (add the pixel entering the window, subtract the one leaving it) rather than resummed from
    /// scratch at every position.
    /// </summary>
    private static unsafe void BoxBlurHorizontal(byte* scan0, int width, int height, int stride, int radius, byte[] rowBuffer)
    {
        var windowSize = (2 * radius) + 1;

        for (var y = 0; y < height; y++)
        {
            var row = scan0 + (y * stride);

            // the window sum needs to read every pixel's *original* value even as this same
            // pass overwrites earlier pixels in the row with their blurred result, so snapshot
            // the row first
            for (var x = 0; x < width; x++)
            {
                var i = x * 4;
                rowBuffer[i] = row[i];
                rowBuffer[i + 1] = row[i + 1];
                rowBuffer[i + 2] = row[i + 2];
                rowBuffer[i + 3] = row[i + 3];
            }

            for (var channel = 0; channel < 4; channel++)
            {
                var sum = 0;
                for (var k = -radius; k <= radius; k++)
                {
                    sum += rowBuffer[(Math.Clamp(k, 0, width - 1) * 4) + channel];
                }

                for (var x = 0; x < width; x++)
                {
                    row[(x * 4) + channel] = (byte)(sum / windowSize);

                    var addX = Math.Clamp(x + radius + 1, 0, width - 1);
                    var removeX = Math.Clamp(x - radius, 0, width - 1);
                    sum += rowBuffer[(addX * 4) + channel] - rowBuffer[(removeX * 4) + channel];
                }
            }
        }
    }

    /// <summary>
    /// Same as <see cref="BoxBlurHorizontal"/>, but averaging each column instead of each row.
    /// </summary>
    private static unsafe void BoxBlurVertical(byte* scan0, int width, int height, int stride, int radius, byte[] columnBuffer)
    {
        var windowSize = (2 * radius) + 1;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var src = (y * stride) + (x * 4);
                var dst = y * 4;
                columnBuffer[dst] = scan0[src];
                columnBuffer[dst + 1] = scan0[src + 1];
                columnBuffer[dst + 2] = scan0[src + 2];
                columnBuffer[dst + 3] = scan0[src + 3];
            }

            for (var channel = 0; channel < 4; channel++)
            {
                var sum = 0;
                for (var k = -radius; k <= radius; k++)
                {
                    sum += columnBuffer[(Math.Clamp(k, 0, height - 1) * 4) + channel];
                }

                for (var y = 0; y < height; y++)
                {
                    scan0[(y * stride) + (x * 4) + channel] = (byte)(sum / windowSize);

                    var addY = Math.Clamp(y + radius + 1, 0, height - 1);
                    var removeY = Math.Clamp(y - radius, 0, height - 1);
                    sum += columnBuffer[(addY * 4) + channel] - columnBuffer[(removeY * 4) + channel];
                }
            }
        }
    }
}
