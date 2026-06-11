using System.Drawing;

namespace FancyMouse.Drawing.Bezels;

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
internal sealed class BezelRenderer : IDisposable
{
    // ── Configuration (all set at construction, never mutated) ───────────────
#pragma warning disable SA1306
    private readonly Color BezelColor;
    private readonly int BezelThickness;    // outer corner radius = ring width in pixels
    private readonly int ThreeDEffectDepth; // 3-D effect layer depth at each ring edge
    private readonly double FadeStart;         // degrees from edge where corner rolloff begins
    private readonly double FadeEnd;           // degrees where rolloff reaches zero
    private readonly double HlMax;             // peak highlight opacity fraction
    private readonly double ShMax;             // peak shadow   opacity fraction
    private readonly float EdgeFadeFraction;  // fraction of vertical edge with secondary effect
#pragma warning restore SA1306

    // ── Corner atlas (owned; built eagerly at construction) ──────────────────
    // 2N×2N sprite sheet with pre-rendered corners and baked-in 3-D effects.
    // Layout: TL=(0,0)  TR=(N,0)  BL=(0,N)  BR=(N,N)  where N=BezelThickness.
    private readonly Bitmap _cornerAtlas;

    public BezelRenderer(
        Color bezelColor,
        int bezelThickness,
        int threeDEffectDepth,
        double fadeStart,
        double fadeEnd,
        double hlMax,
        double shMax,
        float edgeFadeFraction)
    {
        BezelColor = bezelColor;
        BezelThickness = bezelThickness;
        ThreeDEffectDepth = threeDEffectDepth;
        FadeStart = fadeStart;
        FadeEnd = fadeEnd;
        HlMax = hlMax;
        ShMax = shMax;
        EdgeFadeFraction = edgeFadeFraction;

        _cornerAtlas = CornerTemplates.GetCornerTemplates(
            bezelThickness,
            threeDEffectDepth,
            bezelColor,
            fadeStart,
            fadeEnd,
            hlMax,
            shMax);
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
        BezelGraphics.DrawBezelEdges(
            g,
            x,
            y,
            width,
            height,
            BezelThickness,
            ThreeDEffectDepth,
            BezelColor,
            HlMax,
            ShMax,
            EdgeFadeFraction);

        // ── Corners (flat fill + 3-D effects baked in) ────────────────────────
        // Drawn last so the antialiased outer-edge pixels composite correctly
        // over whatever was drawn in the edge strips above.
        var n = BezelThickness;
        var corners = new[]
        {
            // (source region in atlas,      destination on target)
            (src: new Rectangle(0, 0, n, n), dest: new Rectangle(x,             y,              n, n)),  // TL
            (src: new Rectangle(n, 0, n, n), dest: new Rectangle(x + width - n, y,              n, n)),  // TR
            (src: new Rectangle(0, n, n, n), dest: new Rectangle(x,             y + height - n, n, n)),  // BL
            (src: new Rectangle(n, n, n, n), dest: new Rectangle(x + width - n, y + height - n, n, n)),  // BR
        };
        foreach (var (src, dest) in corners)
            g.DrawImage(_cornerAtlas, dest, src, GraphicsUnit.Pixel);
    }

    /* ── Disposal ──────────────────────────────────────────────────────────── */

    public void Dispose()
    {
        _cornerAtlas.Dispose();
    }
}
