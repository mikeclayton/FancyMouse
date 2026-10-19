using System.Drawing;

using SkiaSharp;

namespace FancyMouse.Drawing.Helpers;

/// <summary>
/// Small conversions between the System.Drawing.Primitives types the models use (which are
/// cross-platform) and their SkiaSharp equivalents, plus bitmap creation with the pixel
/// formats the rest of the drawing code relies on.
/// </summary>
internal static class SkiaHelper
{
    /// <summary>
    /// Creates a BGRA8888 bitmap - the same byte layout as GDI+'s 32bpp ARGB formats (and as
    /// WinUI's <c>WriteableBitmap</c> pixel buffer) - cleared to fully transparent black, the
    /// same as a newly created GDI+ bitmap. <see cref="SKAlphaType.Premul"/> is the equivalent
    /// of <c>Format32bppPArgb</c>, and <see cref="SKAlphaType.Unpremul"/> of <c>Format32bppArgb</c>.
    /// </summary>
    public static SKBitmap CreateBitmap(int width, int height, SKAlphaType alphaType)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, alphaType));
        bitmap.Erase(SKColor.Empty);
        return bitmap;
    }

    /// <summary>
    /// Wraps <paramref name="bitmap"/>'s pixels in an <see cref="SKImage"/> for drawing, without
    /// copying them - the result is only valid while <paramref name="bitmap"/> is alive and
    /// unmodified, so dispose it straight after drawing.
    /// </summary>
    public static SKImage AsImage(SKBitmap bitmap)
    {
        using var pixmap = bitmap.PeekPixels();
        return SKImage.FromPixels(pixmap);
    }

    public static SKColor ToSKColor(this Color color)
        => new(color.R, color.G, color.B, color.A);

    public static SKRect ToSKRect(this Rectangle rectangle)
        => SKRect.Create(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
}
