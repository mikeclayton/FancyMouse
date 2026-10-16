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
    internal static void DrawFlatBezelRing(
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
    /// Builds the corner atlas from <see cref="BorderStyle"/>, for use at construction.
    /// </summary>
    private Bitmap DrawCornerTemplates()
    {
        var bezelWidth = (int)this.BezelStyle.Left;

        // this.BezelProfile is scale-independent, so the same instance used for the
        // 1× edge rendering is valid here too, even though GetCornerTemplates
        // renders its atlas at 2× supersampling internally.
        return CornerTemplates.GetCornerTemplates(
            bezelWidth,
            this.BezelStyle.Color ?? Color.Transparent,
            this.BezelProfile);
    }
}
