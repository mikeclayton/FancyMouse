using System.Drawing;
using System.Drawing.Drawing2D;
using FancyMouse.Models.Styles;

namespace FancyMouse.Drawing.ContouredBezels;

// ═════════════════════════════════════════════════════════════════════════════
// BezelRenderer
//
// Immutable configuration object for a single bezel style.  Constructed once
// with all style parameters; renders a bezel ring onto a caller-supplied
// Graphics context on demand.
//
// Geometry contract
// ─────────────────
// BezelThickness controls both the outer corner radius and the ring width, so
// the inner content area is always a plain rectangle (inner corner radius = 0).
// ThreeDEffectDepth pixels are consumed at each edge of the ring for the 3-D
// effect, leaving (BezelThickness − 2 × ThreeDEffectDepth) pixels of flat fill
// in the middle.  E.g. thickness=12, depth=3 → 3px highlight, 6px flat, 3px shadow.
//
// Methods
// ───────
//   DrawBezel  — draws the full bezel ring with 3-D corner highlight/shadow
//
// The corner atlas (a 2N×2N sprite sheet of the four pre-rendered corners with
// 3-D effects baked in) is built eagerly at construction time and owned by
// this instance.
// ═════════════════════════════════════════════════════════════════════════════
public sealed class BezelRenderer : IDisposable
{
    private readonly BorderStyle _borderStyle;
    private readonly BezelConfig _config;

    // ── Corner atlas (owned; built eagerly at construction) ──────────────────
    // 2N×2N sprite sheet with pre-rendered corners and baked-in 3-D effects.
    // Layout: TL=(0,0)  TR=(N,0)  BL=(0,N)  BR=(N,N)  where N=Thickness.
    private readonly Bitmap _cornerAtlas;

    public BezelRenderer(BorderStyle borderStyle, BezelConfig config)
    {
        _borderStyle = borderStyle ?? throw new ArgumentNullException(nameof(borderStyle));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _cornerAtlas = CornerTemplates.GetCornerTemplates(borderStyle, config);
    }

    // ── Render ───────────────────────────────────────────────────────────────

    // Draws the bezel ring with the full 3-D highlight/shadow corner effect.
    //
    // Light source is top-left:
    //   TL — double highlight, peak at 45°
    //   BR — double shadow,    peak at 45°
    //   TR — highlight (top) meets shadow (right), both fade at 45°
    //   BL — shadow (bottom) meets highlight (left), both fade at 45°
    //   Inner arc effects are reversed; BR inner highlight is halved.
    public void DrawBezel(Graphics g, int x, int y, int width, int height)
    {
        // ── Straight edge fills + 3-D effects ────────────────────────────────
        // Fills all four strips with flat BezelColor then overlays highlight /
        // shadow gradient effects on the outer and inner depth layers.
        BezelGraphics.DrawBezelEdges(g, x, y, width, height, _borderStyle, _config);

        // ── Corners (flat fill + 3-D effects baked in) ────────────────────────
        // Drawn last so the antialiased outer-edge pixels composite correctly
        // over whatever was drawn in the edge strips above.
        //
        // NearestNeighbor + PixelOffsetMode.Half ensures an exact 1:1 pixel copy.
        // With Half mode the sample point for dest pixel i lands at exactly i,
        // so NearestNeighbor snaps to the correct source pixel without bilinear
        // blurring. Default bilinear samples at i+0.5, shifting the arc edge ~0.5 px
        // inward and creating a visible gap between the corner arc and the edge strips.
        var n = (int)_borderStyle.Left;
        var corners = new[]
        {
            // (source region in atlas,      destination on target)
            (src: new Rectangle(0, 0, n, n), dest: new Rectangle(x,             y,              n, n)),  // TL
            (src: new Rectangle(n, 0, n, n), dest: new Rectangle(x + width - n, y,              n, n)),  // TR
            (src: new Rectangle(0, n, n, n), dest: new Rectangle(x,             y + height - n, n, n)),  // BL
            (src: new Rectangle(n, n, n, n), dest: new Rectangle(x + width - n, y + height - n, n, n)),  // BR
        };

        var savedInterpolation = g.InterpolationMode;
        var savedPixelOffset = g.PixelOffsetMode;

        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        foreach (var (src, dest) in corners)
        {
            g.DrawImage(_cornerAtlas, dest, src, GraphicsUnit.Pixel);
        }

        g.InterpolationMode = savedInterpolation;
        g.PixelOffsetMode = savedPixelOffset;
    }

    /* ── Disposal ──────────────────────────────────────────────────────────── */

    public void Dispose()
    {
        _cornerAtlas.Dispose();
    }
}
