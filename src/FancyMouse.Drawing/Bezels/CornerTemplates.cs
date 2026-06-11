using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FancyMouse.Drawing.Bezels;

// ═════════════════════════════════════════════════════════════════════════════
// CornerTemplates  (static factory)
//
// Creates the two arc-quadrant alpha-mask bitmaps used to apply the 3-D
// highlight/shadow effect at each bezel corner.  The class is stateless —
// it takes all parameters explicitly and returns a new Bitmap on each call.
// Caching and lifetime management are the caller's responsibility.
//
// Template design
// ───────────────
// Each bitmap has a transparent background.  The corner centre is at pixel
// (0, 0) — top-left of the bitmap — and the arc fills the first quadrant
// (adx ≥ 0, ady ≥ 0), drawn with GDI+ antialiasing so partial-alpha pixels
// appear at both arc edges.
//
// All four physical bezel corners reuse the same template shape; the caller
// applies the appropriate sign flip per corner:
//   TL (−adx, −ady)   TR (+adx, −ady)   BR (+adx, +ady)   BL (−adx, +ady)
//
// Two templates are needed because the outer and inner arc zones carry
// different (often opposite) 3-D effects on the same corner — e.g. for TL
// the outer arc is a highlight while the inner arc is a shadow.
//
//   Exterior template — ring quadrant ThreeDEffectDepth pixels wide at the
//                       outer edge  (radii bezelThickness−depth … bezelThickness)
//
//   Interior template — solid quarter-circle ThreeDEffectDepth pixels in radius,
//                       centred at the inner (square) corner of the content area.
//                       Because innerRadius = bezelThickness − bezelThickness = 0
//                       there is no inner exclusion: the shape is a full pie slice.
// ═════════════════════════════════════════════════════════════════════════════
internal static class CornerTemplates
{
    // Returns a new alpha-mask bitmap for the exterior arc zone.
    // Covers the ring of ThreeDEffectDepth pixels at the outer edge of a corner
    // (radii bezelThickness−threeDEffectDepth … bezelThickness).
    // Caller owns and is responsible for disposing the returned Bitmap.
    public static Bitmap CreateExteriorTemplate(int bezelThickness, int threeDEffectDepth)
    {
        var bmp = CreateArcQuadrantBitmap(
            outerRadius: bezelThickness,
            innerRadius: Math.Max(0, bezelThickness - threeDEffectDepth));
        bmp.Save($@"C:\temp\outer_template_{bezelThickness}_{threeDEffectDepth}.png", ImageFormat.Png);
        return bmp;
    }

    // Returns a new alpha-mask bitmap for the interior arc zone.
    // Covers the solid pie of radius ThreeDEffectDepth at the square inner corner
    // (inner corner radius is 0 because bezelThickness = outer radius = border width).
    // Caller owns and is responsible for disposing the returned Bitmap.
    public static Bitmap CreateInteriorTemplate(int bezelThickness, int threeDEffectDepth)
    {
        var bmp = CreateArcQuadrantBitmap(
            outerRadius: Math.Min(threeDEffectDepth, bezelThickness),
            innerRadius: 0);
        bmp.Save($@"C:\temp\inner_template_{bezelThickness}_{threeDEffectDepth}.png", ImageFormat.Png);
        return bmp;
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    // Renders a ring-shaped arc quadrant onto a new transparent-background bitmap.
    //
    // Corner centre is at pixel (0, 0) — top-left of the bitmap.  The filled zone
    // covers the first quadrant (x ≥ 0, y ≥ 0), from outerRadius inward to
    // innerRadius (if > 0), or solid to centre (if innerRadius == 0).
    //
    // Path strategy — circle figures rather than pies
    // ────────────────────────────────────────────────
    // Each figure is a full circle (four quarter-arcs sharing one bounding box),
    // not a pie (arc + lines to centre).  Only the first-quadrant portion of the
    // circle falls within the bitmap bounds; the other three quadrants are clipped.
    //
    // The visible arc is the 0°→90° segment.  In a circle figure this connects at
    // 0° to the 270°→360° arc and at 90° to the 90°→180° arc — both are G1-smooth
    // (tangent-matching) transitions, exactly as in DrawSolidBezel's rounded rect.
    // Pie figures have sharp corners at both endpoints (arc → line-to-centre), which
    // causes GDI+ to compute slightly different antialias coverage there.
    //
    // Uses FillMode.Alternate so both arc edges are antialiased in a single FillPath.
    //
    // Bitmap size: outerRadius × outerRadius.
    // The outermost row/column (pixel index outerRadius) has effectively zero coverage
    // — at the arc tangent point the circle boundary just grazes the pixel corner —
    // so it is excluded from the bitmap.
    private static Bitmap CreateArcQuadrantBitmap(int outerRadius, int innerRadius)
    {
        int size = outerRadius;
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);

        if (outerRadius <= 0)
        {
            // degenerate — return blank bitmap
            return bmp;
        }

        using var g = Graphics.FromImage(bmp);
        GraphicsHelpers.EnableAntialias(g);

        // FillMode.Alternate: outer disc fills (winding=1), inner disc cancels (winding=2).
        using var path = new GraphicsPath(FillMode.Alternate);
        using var brush = new SolidBrush(Color.White);

        AddCircleFigure(path, outerRadius);
        if (innerRadius > 0)
        {
            AddCircleFigure(path, innerRadius);
        }

        g.FillPath(brush, path);

        return bmp;
    }

    // Adds a full circle as four quarter-arc figures to an existing GraphicsPath.
    // Circle is centred at (0, 0) with the given radius.
    //
    // StartFigure/CloseFigure makes this a self-contained sub-path, compatible
    // with FillMode.Alternate when used alongside a second circle figure.
    //
    // Arc order matches MakeRoundedRect in BezelGraphics (TL → TR → BR → BL),
    // so the 0°→90° arc (the one visible in the bitmap's first quadrant) has
    // smooth G1-continuous neighbours at both endpoints — same context as the
    // solid bezel's rounded-rect outer arc.
    private static void AddCircleFigure(GraphicsPath path, int radius)
    {
        int d = 2 * radius;
        path.StartFigure();
        path.AddArc(-radius, -radius, d, d, 180, 90);  // TL: 180°→270°
        path.AddArc(-radius, -radius, d, d, 270, 90);  // TR: 270°→360°
        path.AddArc(-radius, -radius, d, d,   0, 90);  // BR: 0°→90°   ← visible in bitmap
        path.AddArc(-radius, -radius, d, d,  90, 90);  // BL: 90°→180°
        path.CloseFigure();
    }
}
