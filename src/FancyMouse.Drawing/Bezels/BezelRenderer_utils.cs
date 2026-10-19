using System.Drawing;

using FancyMouse.Drawing.Helpers;

using SkiaSharp;

namespace FancyMouse.Drawing.Bezels;

public sealed partial class BezelRenderer
{
    /// <summary>
    /// Draws a solid colored bezel "ring" onto <paramref name="canvas"/> at position (x, y)
    /// with size (width × height), filled with <paramref name="bezelColor"/> and no 3-D effect.
    ///
    /// The outer boundary is a rounded rectangle with radius <paramref name="cornerRadius"/>.
    /// The inner boundary is a plain square-cornered rectangle, inset by
    /// <paramref name="cornerRadius"/> on every side.
    /// </summary>
    private static void DrawFlatBezelRing(
        SKCanvas canvas,
        int x,
        int y,
        int width,
        int height,
        int cornerRadius,
        Color bezelColor)
    {
        // fail hard on invalid geometry rather than silently drawing nothing
        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "width cannot be negative.");
        }

        if (height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "height cannot be negative.");
        }

        if (cornerRadius < 0 || (width < 2 * cornerRadius) || (height < 2 * cornerRadius))
        {
            throw new ArgumentOutOfRangeException(nameof(cornerRadius), "cornerRadius cannot be negative, and cannot be greater than half of width and height.");
        }

        if (width == 0 || height == 0)
        {
            // a zero-width or zero-height ring has nothing to draw
            return;
        }

        var innerWidth = width - (2 * cornerRadius);
        var innerHeight = height - (2 * cornerRadius);

        using var outerPath = BezelRenderer.GetRoundedRectanglePath(x, y, width, height, cornerRadius);
        using var fillPaint = new SKPaint { Color = bezelColor.ToSKColor(), IsAntialias = true };

        // Two-step rendering:
        //
        //   step 1 — fill the entire rounded rectangle (including the central content area rectangle)
        //   step 2 — erase the inner content area rectangle without antialiasing
        //
        //     ______________          ______________
        //    /..............\        /..............\
        //   |................|      |..+----------+..|
        //   |................|      |..|          |..|
        //   |................|      |..|          |..|
        //   |................|      |..+----------+..|
        //   |................|      |................|
        //    \--------------/        \--------------/
        //         step 1                  step 2
        //
        // note - using an even-odd fill to draw the "ring" in one call doesn't
        // work because it leaves antialiased pixels in the corners of the inner
        // rectangle.
        //
        // The two-pass approach with a regular fill avoids the antialaising by
        // removing the entire rectangle in a separate drawing operation.
        canvas.DrawPath(outerPath, fillPaint);

        // SKBlendMode.Src replaces the destination pixels outright (the equivalent of
        // GDI+'s CompositingMode.SourceCopy) rather than blending the transparent colour
        // over them, which would leave them unchanged
        using var clearPaint = new SKPaint
        {
            Color = Color.Transparent.ToSKColor(),
            IsAntialias = false,
            BlendMode = SKBlendMode.Src,
        };

        canvas.DrawRect(SKRect.Create(x + cornerRadius, y + cornerRadius, innerWidth, innerHeight), clearPaint);
    }

    /// <summary>
    /// Creates an <see cref="SKPath"/> for a rectangle with rounded corners.
    /// The caller owns the returned path and is responsible for disposing the return value.
    /// </summary>
    private static SKPath GetRoundedRectanglePath(int x, int y, int width, int height, int cornerRadius)
    {
        var path = new SKPath();
        if (cornerRadius > 0)
        {
            // ArcTo (rather than AddArc, which starts a new contour every time) joins each
            // arc to the end of the previous one with a straight line - the same as GDI+'s
            // GraphicsPath.AddArc
            var d = 2 * cornerRadius;
            path.ArcTo(SKRect.Create(x,             y,              d, d), 180, 90, forceMoveTo: true);  // TL
            path.ArcTo(SKRect.Create(x + width - d, y,              d, d), 270, 90, forceMoveTo: false); // TR
            path.ArcTo(SKRect.Create(x + width - d, y + height - d, d, d),   0, 90, forceMoveTo: false); // BR
            path.ArcTo(SKRect.Create(x,             y + height - d, d, d),  90, 90, forceMoveTo: false); // BL
        }
        else
        {
            path.AddRect(SKRect.Create(x, y, width, height));
        }

        path.Close();
        return path;
    }
}
