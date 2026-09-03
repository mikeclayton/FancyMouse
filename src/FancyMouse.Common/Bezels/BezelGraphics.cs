using System.Drawing;
using System.Drawing.Drawing2D;
using FancyMouse.Models.Styles;

namespace FancyMouse.Common.Bezels;

public static class BezelGraphics
{
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

    /// <summary>
    /// Draws one straight bezel-edge segment as a 1-pixel-wide rectangle with a
    /// 3-stage gradient effect, using SmoothingMode.None for crisp pixel-aligned fills.
    ///
    /// The gradient runs from (x1, y1) toward (x2, y2):
    ///   Stage 1 — strongColor held at full intensity from position 0 to BezelConstants.EdgeGradientStart
    ///   Stage 2 — fade from strongColor to fadeColor between EdgeGradientStart and EdgeGradientEnd
    ///   Stage 3 — fadeColor held flat from BezelConstants.EdgeGradientEnd to the far end
    ///
    /// (x1, y1) is the corner end where the effect peaks; (x2, y2) is the plain end.
    /// To draw an effect that peaks at the far corner, reverse the coordinates.
    /// </summary>
    private static void DrawBezelEdgeLine(
        Graphics g,
        int x1,
        int y1,
        int x2,
        int y2,
        Color fadeColor,
        Color strongColor)
    {
        var edgeBounds = (y1 == y2)
            ? new RectangleF(Math.Min(x1, x2), y1, Math.Abs(x2 - x1), 1f)
            : new RectangleF(x1, Math.Min(y1, y2), 1f, Math.Abs(y2 - y1));

        var savedMode = g.SmoothingMode;
        var savedPixelOffset = g.PixelOffsetMode;

        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;

        // set a default 2-stop gradient brush; this is overwritten with the full 4-stop
        // gradient below if BezelConstants.EdgeGradientStart/EdgeGradientEnd are ever
        // changed to invalid values, so rendering degrades instead of throwing.
        // (note the coordinates are directional to ensure the gradient blends the right way)
        using var brush = new LinearGradientBrush(
            new Point(x1, y1), new Point(x2, y2), strongColor, fadeColor);

        // for gradient fills, the default WrapMode.Tile fills the gradient region as
        // a series of tiles and it antialiases the edge where they join - this means
        // the *start* color of the gradient from the neighbouring tile can bleed into
        // the *end* color of the adjoining tile:
        //
        //     |▓▓▓▓▒▒▒▒░░░░   ▒|▓▓▓▓▒▒▒▒░░░░   ▒|▓▓▓▓▒▒▒▒░░░░   ▒|▓▓▓▓▒▒▒▒░░░░   ▒|
        //                     ^
        //                     WrapMode.Tile simply repeats the gradient brush,
        //                     and anti-aliasing of a high contrast pixel in the
        //                     next (off-screen) tile bleeds back over into the
        //                     previous tile
        //
        // in our case it means the *start* color for the neighbouring (off-screen) tile
        // can bleed into the *end* color of our edge region, causing a 1-pixel wide
        // rendering artifact on the resulting image.
        //
        // to prevent this, TileFlipXY *mirrors* the gradient at both ends so antialiasing
        // with the neighboring tile uses the same colour at the join and there's no
        // rendering artifact.
        //
        //     |▓▓▓▓▒▒▒▒░░░░    |    ░░░░▒▒▒▒▓▓▓▓|▓▓▓▓▒▒▒▒░░░░    |    ░░░░▒▒▒▒▓▓▓▓|
        //                     ^
        //                     WrapMode.TileFlipXy *mirrors* the gradient brush
        //                     so anti-aliasing blends two pixels of the same color
        //                     and there's no visible bleed effect between tiles
        brush.WrapMode = WrapMode.TileFlipXY;

        // only draw lighting effects fade for valid gradient stepping points
        if ((BezelConstants.EdgeGradientEnd > BezelConstants.EdgeGradientStart) && (BezelConstants.EdgeGradientEnd < 1f))
        {
            // overall, the shape of the resulting gradient is 3 stages,
            // transitioning at <EdgeGradientStart>% and <EdgeGradientEnd>%:
            //
            //     * stage 1 - solid:    strongColor
            //     * stage 2 - gradient: strongColor -> fadeColor
            //     * stage 3 - solid:    fadeColor
            //
            //     |solid|            gradient             |  solid  |
            //     |▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒▒▒▒▒▒▒▒▒▒▒▒░░░░░░░░░░░░---------|
            //     0     ^                                 ^         1.0
            //           |                                 |
            //           EdgeGradientStart                 EdgeGradientEnd
            //
            //     * the first solid color is held flat at strongColor to avoid a
            //       visually jarring gradient starting straight away
            //
            //     * the gradient fades from strongColor to fadeColor
            //
            //     * the final solid color is held flat at fadeColor for the rest
            //       of the edge
            brush.InterpolationColors = new ColorBlend(4)
            {
                Colors = [strongColor, strongColor, fadeColor, fadeColor],
                Positions = [0f, BezelConstants.EdgeGradientStart, BezelConstants.EdgeGradientEnd, 1f],
            };
        }

        g.FillRectangle(brush, edgeBounds);

        g.SmoothingMode = savedMode;
        g.PixelOffsetMode = savedPixelOffset;
    }

    /// <summary>
    /// Draws the straight edge rectangles for a bezel ring.
    /// Each edge is drawn as a series of 1-pixel thick lines
    /// with highlight and shadow effect according to the bezel profile.
    /// </summary>
    internal static void DrawBezelEdges(
        Graphics g,
        int x,
        int y,
        int width,
        int height,
        BorderStyle borderStyle,
        IBezelProfile profile)
    {
        var n = (int)borderStyle.Left;
        var d = (int)borderStyle.Depth;
        var bezelColor = borderStyle.Color ?? Color.Transparent;

        // draw the four straight edge strips with the flat border color first
        var savedMode = g.SmoothingMode;
        var savedPixelOffset = g.PixelOffsetMode;
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;
        using (var flatBrush = new SolidBrush(bezelColor))
        {
            g.FillRectangle(flatBrush, x + n,         y,              width - (2 * n), n);                // top
            g.FillRectangle(flatBrush, x + n,         y + height - n, width - (2 * n), n);                // bottom
            g.FillRectangle(flatBrush, x,             y + n,          n,               height - (2 * n)); // left
            g.FillRectangle(flatBrush, x + width - n, y + n,          n,               height - (2 * n)); // right
        }

        g.SmoothingMode = savedMode;
        g.PixelOffsetMode = savedPixelOffset;

        if (d == 0)
        {
            return;
        }

        // pre-compute the vertical and horizontal endpoints for the edge lines
        var horizontalEdgeX1 = x + n;         // left   end of top  / bottom horizontal segments
        var horizontalEdgeX2 = x + width - n; // right  end of top  / bottom horizontal segments
        var verticalEdgeY1 = y + n;           // top    end of left / right  vertical  segments
        var verticalEdgeY2 = y + height - n;  // bottom end of left / right  vertical  segments

        Color Pix(double hl, double sh) => BezelPrimitives.ApplyEffect(hl, sh, bezelColor, BezelConstants.HighlightMax, BezelConstants.ShadowMax);
        Color PixCS(double hl, double sh, double cs) => Pix(hl * cs, sh * cs);

        // ── Outer + inner ring edge effects ───────────────────────────────────────
        // pos=0 is the outermost pixel of the outer ring (arc boundary — full effect)
        // and the innermost pixel of the inner ring (content boundary — full effect);
        // pos=d-1 is the innermost outer-ring pixel and the outermost inner-ring
        // pixel (both approaching the flat-zone junction — fading effect).
        for (var pos = 0; pos < d; pos++)
        {
            var outerTop = y + pos;
            var outerBottom = y + height - pos - 1;
            var outerLeft = x + pos;
            var outerRight = x + width - pos - 1;

            var innerTop = y + n - pos - 1;
            var innerBottom = y + height - n + pos;
            var innerLeft = x + n - pos - 1;
            var innerRight = x + width - n + pos;

            var cs = profile.GetEdgeIntensity(n, pos);

            // Top outer:    HL base, secondary HL from TL corner (left→right)
            BezelGraphics.DrawBezelEdgeLine(
                g: g,
                x1: horizontalEdgeX1,
                y1: outerTop,
                x2: horizontalEdgeX2,
                y2: outerTop,
                fadeColor: PixCS(BezelConstants.EdgeForwardHighlightFade, 0.0, cs),
                strongColor: PixCS(BezelConstants.EdgeForwardHighlightStrong, 0.0, cs));

            // Right outer:  SH base, secondary SH from BR corner (bottom→top)
            BezelGraphics.DrawBezelEdgeLine(
                g: g,
                x1: outerRight,
                y1: verticalEdgeY2,
                x2: outerRight,
                y2: verticalEdgeY1,
                fadeColor: PixCS(0.0, BezelConstants.EdgeReverseShadowFade, cs),
                strongColor: PixCS(0.0, BezelConstants.EdgeReverseShadowStrong, cs));

            // Bottom outer: SH base, secondary SH from BR corner (right→left)
            BezelGraphics.DrawBezelEdgeLine(
                g: g,
                x1: horizontalEdgeX2,
                y1: outerBottom,
                x2: horizontalEdgeX1,
                y2: outerBottom,
                fadeColor: PixCS(0.0, BezelConstants.EdgeReverseShadowFade, cs),
                strongColor: PixCS(0.0, BezelConstants.EdgeReverseShadowStrong, cs));

            // Left outer:   HL base, secondary HL from TL corner (top→bottom)
            BezelGraphics.DrawBezelEdgeLine(
                g: g,
                x1: outerLeft,
                y1: verticalEdgeY1,
                x2: outerLeft,
                y2: verticalEdgeY2,
                fadeColor: PixCS(BezelConstants.EdgeForwardHighlightFade, 0.0, cs),
                strongColor: PixCS(BezelConstants.EdgeForwardHighlightStrong, 0.0, cs));

            // Top inner:    SH base (reversed), secondary SH from TL inner corner
            BezelGraphics.DrawBezelEdgeLine(
                g: g,
                x1: horizontalEdgeX1,
                y1: innerTop,
                x2: horizontalEdgeX2,
                y2: innerTop,
                fadeColor: PixCS(0.0, BezelConstants.EdgeForwardShadowFade, cs),
                strongColor: PixCS(0.0, BezelConstants.EdgeForwardShadowStrong, cs));

            // Right inner:  HL halved base (bottom→top)
            BezelGraphics.DrawBezelEdgeLine(
                g: g,
                x1: innerRight,
                y1: verticalEdgeY2,
                x2: innerRight,
                y2: verticalEdgeY1,
                fadeColor: PixCS(BezelConstants.EdgeReverseHighlightFade, 0.0, cs),
                strongColor: PixCS(BezelConstants.EdgeReverseHighlightStrong, 0.0, cs));

            // Bottom inner: HL halved base (reversed), secondary HL from BR inner corner
            BezelGraphics.DrawBezelEdgeLine(
                g: g,
                x1: horizontalEdgeX2,
                y1: innerBottom,
                x2: horizontalEdgeX1,
                y2: innerBottom,
                fadeColor: PixCS(BezelConstants.EdgeReverseHighlightFade, 0.0, cs),
                strongColor: PixCS(BezelConstants.EdgeReverseHighlightStrong, 0.0, cs));

            // Left inner:   SH base (top→bottom)
            BezelGraphics.DrawBezelEdgeLine(
                g: g,
                x1: innerLeft,
                y1: verticalEdgeY1,
                x2: innerLeft,
                y2: verticalEdgeY2,
                fadeColor: PixCS(0.0, BezelConstants.EdgeForwardShadowFade, cs),
                strongColor: PixCS(0.0, BezelConstants.EdgeForwardShadowStrong, cs));
        }
    }

    /// <summary>
    /// Fills the content area inside a frame border with a 45° linear gradient,
    /// clipped to the inner rectangle boundary.
    /// </summary>
    internal static void DrawFrameBackground(
        Graphics g,
        int bx,
        int by,
        int bw,
        int bh,
        int border,
        Color gradientStart,
        Color gradientEnd)
    {
        int cx = bx + border;
        int cy = by + border;
        int cw = bw - (2 * border);
        int ch = bh - (2 * border);
        if (cw <= 0 || ch <= 0)
        {
            return;
        }

        using var path = new GraphicsPath();
        path.AddRectangle(new Rectangle(cx, cy, cw, ch));
        using var brush = new LinearGradientBrush(
            new Rectangle(cx, cy, cw, ch),
            gradientStart,
            gradientEnd,
            LinearGradientMode.ForwardDiagonal);
        g.FillPath(brush, path);
    }

    /// <summary>
    /// Fills the content area inside a screen border with a solid colour,
    /// clipped to the inner rectangle boundary.
    /// </summary>
    internal static void DrawScreenBackground(
        Graphics g,
        int bx,
        int by,
        int bw,
        int bh,
        int border,
        Color contentColor)
    {
        int cx = bx + border;
        int cy = by + border;
        int cw = bw - (2 * border);
        int ch = bh - (2 * border);
        if (cw <= 0 || ch <= 0)
        {
            return;
        }

        using var path = new GraphicsPath();
        path.AddRectangle(new Rectangle(cx, cy, cw, ch));
        using var brush = new SolidBrush(contentColor);
        g.FillPath(brush, path);
    }

    /// <summary>
    /// Draws a ring shape onto <paramref name="g"/> at position (x, y) with size
    /// (width × height), filled with <paramref name="color"/> and no 3-D effect.
    ///
    /// The outer boundary is a rounded rectangle with radius <paramref name="outerRadius"/>.
    /// The inner boundary is inset by (<paramref name="outerRadius"/> − <paramref name="innerRadius"/>)
    /// and is itself a rounded rectangle with radius <paramref name="innerRadius"/>
    /// (pass 0 for a plain square inner corner).
    /// </summary>
    internal static void DrawFlatBezelRing(
        Graphics g,
        int x,
        int y,
        int width,
        int height,
        int outerRadius,
        int innerRadius,
        Color color)
    {
        GraphicsHelpers.EnableAntialias(g);

        var inset = outerRadius - innerRadius;
        var innerWidth = width - (2 * inset);
        var innerHeight = height - (2 * inset);

        if (innerWidth <= 0 || innerHeight <= 0)
        {
            return;
        }

        using var outerPath = BezelGraphics.GetRoundedRectanglePath(x, y, width, height, outerRadius);
        using var fillBrush = new SolidBrush(color);

        if (innerRadius == 0)
        {
            // Two-pass rendering for a sharp inner corner (innerRadius == 0):
            //   pass 1 — fill the outer rounded shape (includes content area)
            //   pass 2 — erase the inner rectangle with SmoothingMode.None
            //
            // The one-pass FillMode.Alternate approach antialiases the sharp inner
            // corner, leaving a partial-alpha pixel at the boundary of the inner
            // rectangle in GDI+'s PixelOffsetMode.Half coordinate space.  That
            // stray pixel shows as a dot at the arc centre in the corner template.
            // The two-pass approach avoids the boundary ambiguity entirely.
            g.FillPath(fillBrush, outerPath);

            var savedMode = g.SmoothingMode;
            var savedPixelOffset = g.PixelOffsetMode;
            var savedCompositing = g.CompositingMode;
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.None;
            g.CompositingMode = CompositingMode.SourceCopy;
            using var clearBrush = new SolidBrush(Color.Transparent);
            g.FillRectangle(clearBrush, x + inset, y + inset, innerWidth, innerHeight);
            g.SmoothingMode = savedMode;
            g.PixelOffsetMode = savedPixelOffset;
            g.CompositingMode = savedCompositing;
        }
        else
        {
            using var innerPath = BezelGraphics.GetRoundedRectanglePath(x + inset, y + inset, innerWidth, innerHeight, innerRadius);
            using var ringPath = new GraphicsPath(FillMode.Alternate);
            ringPath.AddPath(outerPath, false);
            ringPath.AddPath(innerPath, false);
            g.FillPath(fillBrush, ringPath);
        }
    }
}
