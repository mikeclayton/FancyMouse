using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FancyMouse.Drawing.Bezels;

internal static class BezelGraphics
{
    /// <summary>
    /// Returns an image containing a template for the 4 corners of the bezel.
    /// </summary>
    /// <remarks>
    /// GDI+ doesn't draw perfectly symmetrical or consistent arcs, and rounding
    /// is sometimes even based on the *coordinates* being drawn to rather than
    /// the shape of the path, which means we'd need to apply highlight and shadow
    /// effects to the corners dynamically when every bezel is rendered in order
    /// to ensure a pixel-perfect effect is applied to each corner.
    ///
    /// Our highlight and shadow effects are not massively expensive, so this
    /// wouldn't be a *major* perofrmance problem, *but* we can reduce the CPU
    /// load by pre-rendering the corners and reusing the same template for every
    /// bezel we draw. We can then simply use DrawImage to copy regions from the
    /// template into the correct position for each bezel, and be assured that the
    /// same pixel-perfect effect is applied to every corner.
    /// </remarks>
    /// <returns>
    /// Returns a bitmap containg a 2x2 sprite grid of the bezel corner.
    /// Individual cells in the grid are the size of the "BezelThickness" parameter,
    /// so the full image is "2 × BezelThickness" in width and height. The corners
    /// are arranged in the "obvious" order - the image for the top left corner is
    /// in the top left cell or the grid, etc.
    ///
    /// The template image needs to be recreated if the color, thickness or 3d effect
    /// depth settings change.
    /// </returns>
    internal static Bitmap GetCornerTemplates(
        int bezelThickness,
        int bezel3DDepth,
        Color bezelColor,
        double fadeStartDegrees,
        double fadeEndDegrees,
        double highlightMax,
        double shadowMax)
    {
        var n = bezelThickness;

        // ── Step 1: render a flat bezel on a 3N×3N transparent bitmap ───────────────
        //
        // 3N×3N ensures each arc endpoint connects to a straight edge segment rather
        // than directly to the adjacent arc, giving correct antialiasing results at
        // the arc endpoints (same as the real bezel DrawFlatBezelRing call will see).
        using var flatBezel = new Bitmap(3 * n, 3 * n, PixelFormat.Format32bppArgb);
        using var flatGraphics = Graphics.FromImage(flatBezel);
        BezelGraphics.DrawFlatBezelRing(
            flatGraphics,
            x: 0,
            y: 0,
            width: flatBezel.Width,
            height: flatBezel.Height,
            bezelThickness: bezelThickness,
            bezelColor: bezelColor);
        flatBezel.Save($@"C:\temp\flat_bezel_{n}.png", ImageFormat.Png);

        // ── Step 1b: render the outer-arc alpha mask on a 3N×3N bitmap ───────────────
        //
        // This is a ring whose outer arc radius is N and inner arc radius is N-D,
        // drawn with full GDI+ antialiasing.  Both edges are rounded (shared arc
        // centres), so the inner boundary gets the same smooth coverage ramp that
        // GDI+ produces on the outer boundary — far better than a manual linear ramp.
        //
        // The mask is white-filled so the alpha channel alone encodes coverage:
        //   alpha = 255 → fully inside the outer arc zone (full 3-D effect)
        //   alpha = 0   → fully outside (no effect)
        //   partial      → on one of the two antialiased boundaries
        var d = bezel3DDepth;
        using var outerArcMask = new Bitmap(3 * n, 3 * n, PixelFormat.Format32bppArgb);
        using (var maskGraphics = Graphics.FromImage(outerArcMask))
        {
            GraphicsHelpers.EnableAntialias(maskGraphics);

            // Outer ring path: same rounded-rect as the main bezel (radius N)
            using var outerRingPath = BezelPrimitives.GetRoundedRectanglePath(0, 0, 3 * n, 3 * n, n);

            // Inner cutout path: inset by D, radius N-D — shares arc centres with outerRingPath
            using var innerCutoutPath = BezelPrimitives.GetRoundedRectanglePath(d, d, (3 * n) - (2 * d), (3 * n) - (2 * d), n - d);

            // FillMode.Alternate: outer shape fills, inner shape punches a hole → ring
            using var ringPath = new GraphicsPath(FillMode.Alternate);
            ringPath.AddPath(outerRingPath, false);
            ringPath.AddPath(innerCutoutPath, false);
            using var whiteBrush = new SolidBrush(Color.White);
            maskGraphics.FillPath(whiteBrush, ringPath);
        }

        outerArcMask.Save($@"C:\temp\outer_arc_mask_{n}_{d}.png", ImageFormat.Png);

        // ── Step 1c: render the inner-arc alpha mask on a 3N×3N bitmap ───────────────
        //
        // A ring of thickness D covering the inner arc zone of every corner:
        //   outer boundary — rounded rect with radius D, arc centres at the four inner
        //                     corners (N,N), (2N,N), (N,2N), (2N,2N) of the bezel ring.
        //                     GDI+ antialiases this edge, which is the boundary between
        //                     the inner arc zone and the flat zone.
        //   inner boundary — the content-area square (N, N, N, N), radius=0.
        //                     Square inner corner matches the actual bezel geometry.
        using var innerArcMask = new Bitmap(3 * n, 3 * n, PixelFormat.Format32bppArgb);
        using (var innerMaskGraphics = Graphics.FromImage(innerArcMask))
        {
            GraphicsHelpers.EnableAntialias(innerMaskGraphics);
            using var outerBoundary = BezelPrimitives.GetRoundedRectanglePath(n - d, n - d, n + (2 * d), n + (2 * d), d);
            using var innerBoundary = new GraphicsPath();
            innerBoundary.AddRectangle(new Rectangle(n, n, n, n));
            using var ringPath = new GraphicsPath(FillMode.Alternate);
            ringPath.AddPath(outerBoundary, false);
            ringPath.AddPath(innerBoundary, false);
            using var whiteBrush = new SolidBrush(Color.White);
            innerMaskGraphics.FillPath(whiteBrush, ringPath);
        }

        innerArcMask.Save($@"C:\temp\inner_arc_mask_{n}_{d}.png", ImageFormat.Png);

        // ── Step 2: copy the four N×N corner regions into a 2N×2N template ──────────
        //
        //   +----+----+----+
        //   | TL |    | TR |      +----+----+
        //   +----+----+----+      | TL | TR |
        //   |    |    |    |  =>  +----+----+
        //   +----+----+----+      | BL | BR |
        //   | BL |    | BR |      +----+----+
        //   +----+----+----+
        //
        // The geometric arc centre for all four corners maps to (N, N) in the template.
        // For any template pixel (px, py): dx = px − N, dy = py − N.
        // Signs of (dx, dy) identify the corner quadrant.
        //
        // Corner regions: source coordinates in the 3N×3N flat image → target in 2N×2N atlas.
        // Hoisted here so the same array is reused for both the template copy and the
        // outer-arc mask atlas copy below.
        var copies = new[]
        {
            (source: new Point(0,         0), target: new Point(0, 0)), // TL
            (source: new Point(2 * n,     0), target: new Point(n, 0)), // TR
            (source: new Point(0,     2 * n), target: new Point(0, n)), // BL
            (source: new Point(2 * n, 2 * n), target: new Point(n, n)), // BR
        };

        var templateImage = new Bitmap(2 * n, 2 * n, PixelFormat.Format32bppArgb);
        using (var templateGraphics = Graphics.FromImage(templateImage))
        {
            // Use NearestNeighbor + PixelOffsetMode.Half for an exact 1:1 pixel copy.
            // With Half mode the sample point for dest pixel i is exactly i (not i+0.5),
            // so NearestNeighbor snaps to the correct source pixel with no bilinear blurring.
            // Default Bilinear would sample at i+0.5 — blending adjacent pixels at the arc
            // boundary — which shifts the visual arc edge ~0.5 px inward and creates a
            // visible gap between the corner arc and the flat-fill straight edges.
            templateGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            templateGraphics.PixelOffsetMode = PixelOffsetMode.Half;

            foreach (var (source, target) in copies)
            {
                templateGraphics.DrawImage(
                    flatBezel,
                    destRect: new Rectangle(target.X, target.Y, n, n),
                    srcRect: new Rectangle(source.X, source.Y, n, n),
                    srcUnit: GraphicsUnit.Pixel);
            }
        }

        templateImage.Save($@"C:\temp\template_{n}.png", ImageFormat.Png);

        // Copy the same four corners of the outer-arc mask into a parallel 2N×2N atlas.
        // In Step 3 we read the mask alpha to drive outerFade instead of a linear ramp.
        var outerArcMaskAtlas = new Bitmap(2 * n, 2 * n, PixelFormat.Format32bppArgb);
        using (var maskAtlasGraphics = Graphics.FromImage(outerArcMaskAtlas))
        {
            maskAtlasGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            maskAtlasGraphics.PixelOffsetMode = PixelOffsetMode.Half;
            foreach (var (source, target) in copies)
            {
                maskAtlasGraphics.DrawImage(
                    outerArcMask,
                    destRect: new Rectangle(target.X, target.Y, n, n),
                    srcRect: new Rectangle(source.X, source.Y, n, n),
                    srcUnit: GraphicsUnit.Pixel);
            }
        }

        // Same four corner copies for the inner-arc mask atlas.
        // The inner arc zone pixels near each corner land at (N-1,N-1) / (N,N-1) / (N-1,N) / (N,N)
        // within their respective copy regions, ending up adjacent to the shared centre (N,N) in
        // the atlas — which is exactly where Step 3 reads them using (srcX-N, srcY-N) offsets.
        var innerArcMaskAtlas = new Bitmap(2 * n, 2 * n, PixelFormat.Format32bppArgb);
        using (var innerMaskAtlasGraphics = Graphics.FromImage(innerArcMaskAtlas))
        {
            innerMaskAtlasGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            innerMaskAtlasGraphics.PixelOffsetMode = PixelOffsetMode.Half;
            foreach (var (source, target) in copies)
            {
                innerMaskAtlasGraphics.DrawImage(
                    innerArcMask,
                    destRect: new Rectangle(target.X, target.Y, n, n),
                    srcRect: new Rectangle(source.X, source.Y, n, n),
                    srcUnit: GraphicsUnit.Pixel);
            }
        }

        // we don't need the flat bezel image or the source masks anymore
        flatBezel.Dispose();
        outerArcMask.Dispose();
        innerArcMask.Dispose();

        // ── Step 3: bake 3D highlight/shadow into atlas pixels ───────────────────
        // Walk every pixel; skip transparent (outside ring) and flat-zone pixels.
        // Outer arc zone: radial dist from (N,N) in [N−depth, N]
        // Inner arc zone: radial dist from (N,N) in [0, depth]
        // Flat zone:      between the two — leave as plain bezelColor
        //
        // For partial-coverage outer-edge pixels, the alpha (GDI+ coverage) is
        // preserved unchanged so DrawImage composites them correctly with the
        // background.  Only RGB is replaced with the tinted colour.
        double CornerWeight(double theta) => BezelPrimitives.CornerWeight(theta, fadeStartDegrees, fadeEndDegrees);
        double GdiAngle(int dx, int dy) => BezelPrimitives.GdiAngle(dx, dy);
        double MidpointPeak(double theta) => BezelPrimitives.MidpointPeak(theta);
        double MidpointFade(double theta) => BezelPrimitives.MidpointFade(theta);
        Color ApplyEffect(double hl, double sh, Color baseColor, double hlMax, double shMax) => BezelPrimitives.ApplyEffect(hl, sh, baseColor, hlMax, shMax);

        var templateData = templateImage.LockBits(
            new Rectangle(0, 0, templateImage.Width, templateImage.Height),
            ImageLockMode.ReadWrite,
            PixelFormat.Format32bppArgb);
        var outerArcMaskData = outerArcMaskAtlas.LockBits(
            new Rectangle(0, 0, outerArcMaskAtlas.Width, outerArcMaskAtlas.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);
        var innerArcMaskData = innerArcMaskAtlas.LockBits(
            new Rectangle(0, 0, innerArcMaskAtlas.Width, innerArcMaskAtlas.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        // we've got the *flat* bezel corners in the template now, and we
        // want to add our highlight and shadow effects, which we'll do by
        // drawing partially transparent white (highlight) or black (shadow)
        // pixels on top of the flat bezel pixels.
        //
        // however, the flat bezel already uses transparency for anti-aliasing
        // the outer edge against the transparent image background. we'll
        // process any template pixels that are not 100% transparent, but the
        // effect we apply will only affect the RGB channels, leaving the
        // alpha channel (transparency) as-is. this way the highlight / shadow
        // on semi transparent flat pixels will be proportional in intensity
        // to the flat pixel's visiblity.
        try
        {
            unsafe
            {
                byte* scan0 = (byte*)templateData.Scan0;
                var stride = templateData.Stride;
                byte* maskScan0 = (byte*)outerArcMaskData.Scan0;
                var maskStride = outerArcMaskData.Stride;
                byte* innerMaskScan0 = (byte*)innerArcMaskData.Scan0;
                var innerMaskStride = innerArcMaskData.Stride;
                const int bitsPerPixel = 4; // PixelFormat.Format32bppArgb

                for (var srcY = 0; srcY < templateImage.Height; srcY++)
                {
                    for (var srcX = 0; srcX < templateImage.Width; srcX++)
                    {
                        byte* srcArgb = scan0 + (srcY * stride) + (srcX * bitsPerPixel);
                        if (srcArgb[3] == 0)
                        {
                            // 100% transparent — completely outside the ring
                            continue;
                        }

                        var originOffset = new Point(
                            y: srcY - n,
                            x: srcX - n);
                        var originDistance = Math.Sqrt(
                            (originOffset.X * originOffset.X) + (originOffset.Y * originOffset.Y));

                        // outerFade: read from the outer-arc mask atlas.
                        // The mask was rendered with full GDI+ antialiasing, so its alpha channel
                        // encodes smooth coverage at both the outer and inner arc boundaries —
                        // no manual ramp needed.  Pixels outside the outer arc zone (mask alpha=0)
                        // are skipped; the +1 guard on originDistance catches the partial-alpha
                        // antialias pixels GDI+ places just outside the nominal radius N.
                        byte* maskArgb = maskScan0 + (srcY * maskStride) + (srcX * bitsPerPixel);
                        var outerFade = maskArgb[3] / 255.0;

                        // inOuterArc: any pixel the outer mask has coverage for.
                        var inOuterArc = outerFade > 0.0;

                        // innerFade: read from the inner-arc mask atlas (same alpha-channel approach).
                        // The mask's outer arc (radius D from each inner corner) gives GDI+ antialias
                        // at the boundary between the inner arc zone and the flat zone.
                        byte* innerMaskArgb = innerMaskScan0 + (srcY * innerMaskStride) + (srcX * bitsPerPixel);
                        var innerFade = innerMaskArgb[3] / 255.0;
                        var inInnerArc = innerFade > 0.0;

                        if (!inOuterArc && !inInnerArc)
                        {
                            // flat zone between lighting effects — leave as plain bezelColor
                            continue;
                        }

                        var arcFade = inOuterArc ? outerFade : innerFade;

                        double theta;
                        var hl = 0.0;
                        var sh = 0.0;

                        if ((originOffset.X < 0) && (originOffset.Y < 0))
                        {
                            // pixel is in the top-left quadrant
                            // highlight and shadow effects overlap and combine to make a
                            // TL — top+left both highlight → outer double-HL, inner double-SH
                            theta = 270.0 - GdiAngle(originOffset.X, originOffset.Y);
                            var w = CornerWeight(theta) + CornerWeight(90 - theta) + 0.5 + (0.75 * MidpointPeak(theta));
                            if (inOuterArc)
                            {
                                hl = w;
                            }
                            else
                            {
                                sh = w;
                            }
                        }
                        else if ((originOffset.X >= 0) && (originOffset.Y < 0))
                        {
                            // TR — top=HL meets right=SH, both fade to flat at 45°
                            theta = (GdiAngle(originOffset.X, originOffset.Y) - 270.0 + 360.0) % 360.0;
                            if (inOuterArc)
                            {
                                hl = CornerWeight(theta) * MidpointFade(theta);
                                sh = CornerWeight(90 - theta) * MidpointFade(90 - theta);
                            }
                            else
                            {
                                hl = CornerWeight(90 - theta) * MidpointFade(90 - theta);
                                sh = CornerWeight(theta) * MidpointFade(theta);
                            }
                        }
                        else if (originOffset.X >= 0 && originOffset.Y >= 0)
                        {
                            // BR — bottom+right both shadow → outer double-SH, inner halved-HL
                            // (inner HL halved to avoid over-brightness against outer-BR shadow)
                            theta = 90.0 - GdiAngle(originOffset.X, originOffset.Y);
                            if (inOuterArc)
                            {
                                sh = CornerWeight(theta) + CornerWeight(90 - theta) + 0.5 + (0.75 * MidpointPeak(theta));
                            }
                            else
                            {
                                hl = (0.5 * CornerWeight(theta)) + (0.5 * CornerWeight(90 - theta)) + 0.25 + (0.375 * MidpointPeak(theta));
                            }
                        }
                        else
                        {
                            // originOffset.X < 0 && originOffset.Y >= 0
                            // BL — bottom=SH meets left=HL, both fade to flat at 45°
                            theta = GdiAngle(originOffset.X, originOffset.Y) - 90.0;
                            if (inOuterArc)
                            {
                                hl = CornerWeight(90 - theta) * MidpointFade(90 - theta);
                                sh = CornerWeight(theta) * MidpointFade(theta);
                            }
                            else
                            {
                                hl = CornerWeight(theta) * MidpointFade(theta);
                                sh = CornerWeight(90 - theta) * MidpointFade(90 - theta);
                            }
                        }

                        var newColor = ApplyEffect(hl * arcFade, sh * arcFade, bezelColor, highlightMax, shadowMax);
                        srcArgb[0] = newColor.B;
                        srcArgb[1] = newColor.G;
                        srcArgb[2] = newColor.R;

                        // srcArgb[3] (alpha) intentionally unchanged — preserves antialiased
                        // outer-edge coverage for correct DrawImage compositing
                    }
                }
            }
        }
        finally
        {
            templateImage.UnlockBits(templateData);
            outerArcMaskAtlas.UnlockBits(outerArcMaskData);
            innerArcMaskAtlas.UnlockBits(innerArcMaskData);
        }

        outerArcMaskAtlas.Dispose();
        innerArcMaskAtlas.Dispose();

        templateImage.Save(
            $@"C:\temp\corner_atlas_{n}_{bezel3DDepth}.png",
            ImageFormat.Png);

        return templateImage;
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
    internal static void DrawBezelEdge(
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
        g.SmoothingMode = SmoothingMode.None;

        using var brush = new LinearGradientBrush(
            new Point(x1, y1), new Point(x2, y2), cornerColor, baseColor);
        if (fadeFraction > 0f && fadeFraction < 1f)
        {
            const float plateau = 0.05f;
            var blend = new ColorBlend(4);
            blend.Colors = new[] { cornerColor, cornerColor, baseColor, baseColor };
            blend.Positions = new[] { 0f, plateau, fadeFraction, 1f };
            brush.InterpolationColors = blend;
        }

        g.FillRectangle(brush, edgeBounds);

        g.SmoothingMode = savedMode;
    }

    /// <summary>
    /// Draws the 3-D highlight / shadow edge effects for all four outer-ring and
    /// four inner-ring straight edge segments of a bezel.
    ///
    /// Light source is top-left:
    ///   Outer top / left edges    — highlight (HL)
    ///   Outer bottom / right edges — shadow   (SH)
    ///   Inner edges are reversed; inner bottom / right carry a halved highlight.
    ///
    /// Each edge is drawn as one pixel per depth layer (depth iterations total),
    /// working from the outermost layer inward for the outer ring and from the
    ///  innermost layer outward for the inner ring.  Flat-zone pixels in the middle
    /// of the ring are untouched; they should already be filled with BezelColor.
    ///
    /// Corner arc endpoints define the start/end of each straight segment, and are
    /// constant across all depth layers (arc-centre-to-arc-centre span).
    /// </summary>
    internal static void DrawBezelEdges(
        Graphics g,
        int x,
        int y,
        int width,
        int height,
        int bezelThickness,
        int bezel3DDepth,
        Color bezelColor,
        double hlMax,
        double shMax,
        double edgeFadeFraction)
    {
        var n = bezelThickness;
        var d = bezel3DDepth;

        // Pre-compute straight-edge span endpoints (constant across all depth layers).
        // These are the x/y positions of the arc centres at the corners of the bezel,
        // i.e. where the arcs end and the straight segments begin.
        // Outer span: arc-centre to arc-centre, inclusive on both sides.
        // DrawBezelEdge uses |x2−x1| as the rectangle width, so x2 is EXCLUSIVE —
        // setting outerTrX = x+width−N means the filled rectangle ends at x+width−N−1,
        // which is exactly the last pixel before the TR/BR corner zone.
        var outerTlX = x + n;               // left  end of outer top/bottom segments (inclusive)
        var outerTrX = x + width - n;       // right end (exclusive — rectangle ends at outerTrX−1)
        var outerTlY = y + n;               // top   end of outer left/right segments (inclusive)
        var outerBlY = y + height - n;      // bottom end (exclusive — rectangle ends at outerBlY−1)

        // Inner span: starts at the first pixel OUTSIDE the corner zone (x+N, y+N),
        // not at x+N−1 / y+N−1 which would overlap the corner atlas region drawn last.
        var innerTlX = x + n;               // left  end of inner top/bottom segments (inclusive)
        var innerTrX = x + width - n;       // right end (exclusive)
        var innerTlY = y + n;               // top   end of inner left/right segments (inclusive)
        var innerBlY = y + height - n;      // bottom end (exclusive)

        Color Pix(double hl, double sh) => BezelPrimitives.ApplyEffect(hl, sh, bezelColor, hlMax, shMax);

        // ── Outer ring edge effects ───────────────────────────────────────────────
        // when d2 = 0,   d2 is the outermost pixel row/column
        // when d2 = d-1, d2 is the innermost effect layer.
        for (var d2 = 0; d2 < d; d2++)
        {
            var top = y + d2;
            var bottom = y + height - d2 - 1;
            var left = x + d2;
            var right = x + width - d2 - 1;

            // Top outer:    HL base, secondary HL from TL corner (left→right)
            DrawBezelEdge(g, outerTlX, top,    outerTrX, top,    Pix(1.0, 0.0), Pix(1.5, 0.0), 0.33f);

            // Bottom outer: SH base, secondary SH from BR corner (right→left)
            DrawBezelEdge(g, outerTrX, bottom, outerTlX, bottom, Pix(0.0, 1.0), Pix(0.0, 1.5), 0.33f);

            // Left outer:   HL base, secondary HL from TL corner (top→bottom)
            DrawBezelEdge(g, left, outerTlY,   left, outerBlY,   Pix(1.0, 0.0), Pix(1.5, 0.0), (float)edgeFadeFraction);

            // Right outer:  SH base, secondary SH from BR corner (bottom→top)
            DrawBezelEdge(g, right, outerBlY,  right, outerTlY,  Pix(0.0, 1.0), Pix(0.0, 1.5), (float)edgeFadeFraction);
        }

        // ── Inner ring edge effects ───────────────────────────────────────────────
        // when d2 = 0,   d2 is the innermost pixel row/column (adjacent to content area);
        // when d2 = D-1, d2 is the outermost effect layer inside the ring.
        for (var d2 = 0; d2 < d; d2++)
        {
            var iTop = y + n - d2 - 1;
            var iBottom = y + height - n + d2;
            var iLeft = x + n - d2 - 1;
            var iRight = x + width - n + d2;

            // Top inner:    SH base (reversed), secondary SH from TL inner corner
            DrawBezelEdge(g, innerTlX, iTop,    innerTrX, iTop,    Pix(0.0, 1.0), Pix(0.0, 1.5), 0.33f);

            // Bottom inner: HL halved base (reversed), secondary HL from BR inner corner
            DrawBezelEdge(g, innerTrX, iBottom, innerTlX, iBottom, Pix(0.5, 0.0), Pix(1.0, 0.0), 0.33f);

            // Left inner:   SH base (top→bottom)
            DrawBezelEdge(g, iLeft, innerTlY,   iLeft, innerBlY,   Pix(0.0, 1.0), Pix(0.0, 1.5), (float)edgeFadeFraction);

            // Right inner:  HL halved base (bottom→top)
            DrawBezelEdge(g, iRight, innerBlY,  iRight, innerTlY,  Pix(0.5, 0.0), Pix(0.75, 0.0), (float)edgeFadeFraction);
        }
    }

    /// <summary>
    /// Draws the bezel ring filled in BezelColor onto an existing Graphics context
    /// at position (x, y) with size (width × height), no 3-D effect applied.
    ///
    /// Save/Restore brackets the clip change so the caller's clip is preserved.
    /// </summary>
    internal static void DrawFlatBezelRing(
        Graphics g,
        int x,
        int y,
        int width,
        int height,
        int bezelThickness,
        Color bezelColor)
    {
        GraphicsHelpers.EnableAntialias(g);

        var innerWidth = width - (2 * bezelThickness);
        var innerHeight = height - (2 * bezelThickness);

        if (innerWidth <= 0 || innerHeight <= 0)
        {
            return;
        }

        // create the outer rectangle with rounded corners that represents
        // the outside of the bezel. (The inner rectangle will be subtracted from this)
        using var outerPath = BezelPrimitives.GetRoundedRectanglePath(
            x,
            y,
            width,
            height,
            r: bezelThickness);

        // create the inner rectangle that represents the screen inside the bezel
        using var innerPath = new GraphicsPath();
        innerPath.AddRectangle(new Rectangle(
            x + bezelThickness,
            y + bezelThickness,
            innerWidth,
            innerHeight));

        using var brush = new SolidBrush(bezelColor);

        var state = g.Save();
        g.SetClip(innerPath, CombineMode.Exclude);
        g.FillPath(brush, outerPath);
        g.Restore(state);
    }
}
