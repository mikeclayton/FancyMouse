using System.Drawing;
using System.Drawing.Drawing2D;
using FancyMouse.Models.Styles;

namespace FancyMouse.Drawing.Bezels;

internal static class BezelGraphics
{
    /// <summary>
    /// Creates a graphics path for a rectangle with rounded corners.
    /// </summary>
    private static GraphicsPath GetRoundedRectanglePath(int x, int y, int w, int h, int r)
    {
        var path = new GraphicsPath();
        if (r > 0)
        {
            var d = 2 * r;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
        }
        else
        {
            path.AddRectangle(new Rectangle(x, y, w, h));
        }

        path.CloseFigure();
        return path;
    }

    /// <summary>
    /// Draws one straight bezel-edge segment as a 1-pixel-wide rectangle with a
    /// 3-stage gradient effect, using SmoothingMode.None for crisp pixel-aligned fills.
    ///
    /// The gradient runs from (x1, y1) toward (x2, y2):
    ///   Stage 1 — cornerColor held at full intensity from position 0 to the plateau (5 %)
    ///   Stage 2 — fade from cornerColor to baseColor between the plateau and fadeFraction
    ///   Stage 3 — baseColor held flat from fadeFraction to the far end
    ///
    /// (x1, y1) is the corner end where the effect peaks; (x2, y2) is the plain end.
    /// To draw an effect that peaks at the far corner, reverse the coordinates.
    /// </summary>
    private static void DrawBezelEdge(
        Graphics g,
        int x1,
        int y1,
        int x2,
        int y2,
        Color baseColor,
        Color cornerColor,
        float fadeFraction)
    {
        var edgeBounds = (y1 == y2)
            ? new RectangleF(Math.Min(x1, x2), y1, Math.Abs(x2 - x1), 1f)
            : new RectangleF(x1, Math.Min(y1, y2), 1f, Math.Abs(y2 - y1));

        var savedMode = g.SmoothingMode;
        var savedPixelOffset = g.PixelOffsetMode;

        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;

        using var brush = new LinearGradientBrush(
            new Point(x1, y1), new Point(x2, y2), cornerColor, baseColor);

        // for gradient fills, the default WrapMode.Tile fills the gradient region as
        // a series of tiles and it antialiases the edge where they join - this means
        // the *end* color of the gradient from the neighbouring tile can bleed into
        // the *start* color of the adjoining tile.
        //
        // in our case it means the *end* color for the neighbouring (off-screen) tile
        // can bleed into the *start* color of our edge region, causing a 1-pixel wide
        // rendering artifact on the resulting image.
        //
        // to prevent this, TileFlipXY *mirrors* the gradient at both ends so antialiasing
        // with the neighboring tile uses the same colour at the join and there's no
        // rendering artifact.
        brush.WrapMode = WrapMode.TileFlipXY;

        // a fixed percentage along the line before the lighting effect begins to fade
        const float fadePlateau = 0.05f;

        // only draw lighting effects fade for valid gradient stepping points
        if ((fadeFraction > fadePlateau) && (fadeFraction < 1f))
        {
            // don't change the effect level for the first 5% of the edge,
            // to avoid visually jarring gradients straight after the corner
            var blend = new ColorBlend(4);
            blend.Colors = new[] { cornerColor, cornerColor, baseColor, baseColor };
            blend.Positions = new[] { 0f, fadePlateau, fadeFraction, 1f };
            brush.InterpolationColors = blend;
        }

        g.FillRectangle(brush, edgeBounds);

        g.SmoothingMode = savedMode;
        g.PixelOffsetMode = savedPixelOffset;
    }

    /// <summary>
    /// Draws all four straight edge segments of a bezel ring: first a flat
    /// <paramref name="bezelColor"/> fill across the full strip width, then 3-D
    /// highlight / shadow effects overlaid on the outer and inner depth layers.
    ///
    /// Light source is top-left:
    ///   Outer top / left edges     — highlight (HL)
    ///   Outer bottom / right edges — shadow   (SH)
    ///   Inner edges are reversed; inner bottom / right carry a halved highlight.
    ///
    /// Each effect layer is one pixel wide, working from the outermost layer
    /// inward for the outer ring and from the innermost layer outward for the
    /// inner ring.  Flat-zone pixels in the middle are filled with
    /// <paramref name="bezelColor"/> by the flat-fill pass.
    ///
    /// Corner arc endpoints define the start/end of each straight segment, and are
    /// constant across all depth layers (arc-centre-to-arc-centre span).
    /// </summary>
    internal static void DrawBezelEdges(
        NLog.ILogger logger,
        Graphics g,
        int x,
        int y,
        int width,
        int height,
        BorderStyle borderStyle,
        BezelConfig config)
    {
        var n = (int)borderStyle.Left;
        var d = (int)borderStyle.Depth;
        var bezelColor = borderStyle.Color ?? Color.Transparent;

        // ── Straight edge flat fills ──────────────────────────────────────────
        // Fill the four straight edge strips with flat bezelColor first;
        // the 3-D effect passes below then overlay highlight / shadow on top.
        var savedMode = g.SmoothingMode;
        var savedPixelOffset = g.PixelOffsetMode;
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.None;
        using (var flatBrush = new SolidBrush(bezelColor))
        {
            g.FillRectangle(flatBrush, x + n,             y,              width - (2 * n),  n);              // top
            g.FillRectangle(flatBrush, x + n,             y + height - n, width - (2 * n),  n);              // bottom
            g.FillRectangle(flatBrush, x,                 y + n,          n,                height - (2 * n)); // left
            g.FillRectangle(flatBrush, x + width - n,     y + n,          n,                height - (2 * n)); // right
        }

        g.SmoothingMode = savedMode;
        g.PixelOffsetMode = savedPixelOffset;

        if (d == 0)
        {
            return;
        }

        // Pre-compute straight-edge span endpoints (constant across all depth layers).
        // These are the x/y positions of the arc centres at the corners of the bezel,
        // i.e. where the arcs end and the straight segments begin.
        // Outer span: arc-centre to arc-centre, inclusive on both sides.
        // DrawBezelEdge uses |x2−x1| as the rectangle width, so x2 is EXCLUSIVE —
        // setting outerTrX = x+width−N means the filled rectangle ends at x+width−N−1,
        // which is exactly the last pixel before the TR/BR corner zone.
        var horizontalEdgeX0 = x + n;         // left  end of outer top/bottom segments (inclusive)
        var horizontalEdgeX1 = x + width - n; // right end (exclusive — rectangle ends at outerTrX−1)
        var verticalEdgeY0 = y + n;           // top   end of outer left/right segments (inclusive)
        var verticalEdgeY1 = y + height - n;  // bottom end (exclusive — rectangle ends at outerBlY−1)

        Color Pix(double hl, double sh) => BezelPrimitives.ApplyEffect(hl, sh, bezelColor, config.HighlightMax, config.ShadowMax);
        Color PixCS(double hl, double sh, double cs) => Pix(hl * cs, sh * cs);

        var profile = new BezelProfile(n, d);

        // ── Outer ring edge effects ───────────────────────────────────────────────
        // d2=0 is the outermost pixel (arc boundary — full effect);
        // d2=d-1 is the innermost (approaches flat-zone junction — fading effect).
        for (var d2 = 0; d2 < d; d2++)
        {
            var outerTop = y + d2;
            var outerBottom = y + height - d2 - 1;
            var outerLeft = x + d2;
            var outerRight = x + width - d2 - 1;

            var cs = profile.GetEdgeIntensity(d2);

            // Top outer:    HL base, secondary HL from TL corner (left→right)
            DrawBezelEdge(g, horizontalEdgeX0, outerTop, horizontalEdgeX1, outerTop, PixCS(1.0, 0.0, cs), PixCS(1.5, 0.0, cs), config.EdgeFadeFraction);

            // Right outer:  SH base, secondary SH from BR corner (bottom→top)
            DrawBezelEdge(g, outerRight, verticalEdgeY1, outerRight, verticalEdgeY0, PixCS(0.0, 1.0, cs), PixCS(0.0, 1.5, cs), config.EdgeFadeFraction);

            // Bottom outer: SH base, secondary SH from BR corner (right→left)
            DrawBezelEdge(g, horizontalEdgeX1, outerBottom, horizontalEdgeX0, outerBottom, PixCS(0.0, 1.0, cs), PixCS(0.0, 1.5, cs), config.EdgeFadeFraction);

            // Left outer:   HL base, secondary HL from TL corner (top→bottom)
            DrawBezelEdge(g, outerLeft, verticalEdgeY0, outerLeft, verticalEdgeY1, PixCS(1.0, 0.0, cs), PixCS(1.5, 0.0, cs), config.EdgeFadeFraction);
        }

        // ── Inner ring edge effects ───────────────────────────────────────────────
        // d2=0 is the innermost pixel (content boundary — full effect);
        // d2=d-1 is the outermost (approaches flat-zone junction — fading effect).
        for (var d2 = 0; d2 < d; d2++)
        {
            var innerTop = y + n - d2 - 1;
            var innerBottom = y + height - n + d2;
            var innerLeft = x + n - d2 - 1;
            var innerRight = x + width - n + d2;

            var cs = profile.GetEdgeIntensity(d2);

            // Top inner:    SH base (reversed), secondary SH from TL inner corner
            DrawBezelEdge(g, horizontalEdgeX0, innerTop, horizontalEdgeX1, innerTop, PixCS(0.0, 1.0, cs), PixCS(0.0, 1.5, cs), config.EdgeFadeFraction);

            // Right inner:  HL halved base (bottom→top)
            DrawBezelEdge(g, innerRight, verticalEdgeY1, innerRight, verticalEdgeY0, PixCS(0.5, 0.0, cs), PixCS(0.75, 0.0, cs), config.EdgeFadeFraction);

            // Bottom inner: HL halved base (reversed), secondary HL from BR inner corner
            DrawBezelEdge(g, horizontalEdgeX1, innerBottom, horizontalEdgeX0, innerBottom, PixCS(0.5, 0.0, cs), PixCS(0.75, 0.0, cs), config.EdgeFadeFraction);

            // Left inner:   SH base (top→bottom)
            DrawBezelEdge(g, innerLeft, verticalEdgeY0, innerLeft, verticalEdgeY1, PixCS(0.0, 1.0, cs), PixCS(0.0, 1.5, cs), config.EdgeFadeFraction);
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
