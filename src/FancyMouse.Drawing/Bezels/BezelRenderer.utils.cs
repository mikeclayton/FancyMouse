using System.Drawing;
using System.Drawing.Drawing2D;

namespace FancyMouse.Drawing.Bezels;

public sealed partial class BezelRenderer
{
    /// <summary>
    /// Draws a solid colored bezel "ring" onto <paramref name="g"/> at position (x, y)
    /// with size (width × height), filled with <paramref name="bezelColor"/> and no 3-D effect.
    ///
    /// The outer boundary is a rounded rectangle with radius <paramref name="cornerRadius"/>.
    /// The inner boundary is a plain square-cornered rectangle, inset by
    /// <paramref name="cornerRadius"/> on every side.
    /// </summary>
    private static void DrawFlatBezelRing(
        Graphics g,
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

        BezelRenderer.EnableAntialias(g);

        using var outerPath = BezelRenderer.GetRoundedRectanglePath(x, y, width, height, cornerRadius);
        using var fillBrush = new SolidBrush(bezelColor);

        // Two-step rendering:
        //
        //   step 1 — fill the entire rounded rectangle (including the central content area rectangle)
        //   step 2 — erase the inner content area rectangle with SmoothingMode.None
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
        // note - using FillMode.Alternate to draw the "ring" in one gdi call
        // doesn't work because it leaves antialiased pixels in the corners of
        // the inner rectangle in GDI+'s PixelOffsetMode.Half coordinate space.
        //
        // The two-pass approach with a regular fill avoids the antialaising by
        // removing the entire rectangle in a separate drawing operation.
        g.FillPath(fillBrush, outerPath);

        var savedMode = g.SmoothingMode;
        var savedPixelOffset = g.PixelOffsetMode;
        var savedCompositing = g.CompositingMode;

        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;
        g.CompositingMode = CompositingMode.SourceCopy;

        using var clearBrush = new SolidBrush(Color.Transparent);

        g.FillRectangle(clearBrush, x + cornerRadius, y + cornerRadius, innerWidth, innerHeight);
        g.SmoothingMode = savedMode;
        g.PixelOffsetMode = savedPixelOffset;
        g.CompositingMode = savedCompositing;
    }

    /// <summary>
    /// Enables antialiased drawing on <paramref name="g"/>, with pixel offsets
    /// aligned to whole pixels rather than fractional (0.5, 0.5) coordinates.
    /// </summary>
    private static void EnableAntialias(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;
    }

    /// <summary>
    /// Creates a <see cref="GraphicsPath"/> for a rectangle with rounded corners.
    /// The caller owns the returned path and is responsible for disposing the return value.
    /// </summary>
    private static GraphicsPath GetRoundedRectanglePath(int x, int y, int width, int height, int cornerRadius)
    {
        var path = new GraphicsPath();
        if (cornerRadius > 0)
        {
            var d = 2 * cornerRadius;
            path.AddArc(x,             y,              d, d, 180, 90); // TL
            path.AddArc(x + width - d, y,              d, d, 270, 90); // TR
            path.AddArc(x + width - d, y + height - d, d, d,   0, 90); // BR
            path.AddArc(x,             y + height - d, d, d,  90, 90); // BL
        }
        else
        {
            path.AddRectangle(new Rectangle(x, y, width, height));
        }

        path.CloseFigure();
        return path;
    }
}
