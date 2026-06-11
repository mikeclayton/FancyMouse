using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BezelPreview.Drawing;

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
    public readonly Color  BezelColor;
    public readonly int    BezelThickness;    // outer corner radius = ring width in pixels
    public readonly int    ThreeDEffectDepth; // 3-D effect layer depth at each ring edge
    public readonly double FadeStart;         // degrees from edge where corner rolloff begins
    public readonly double FadeEnd;           // degrees where rolloff reaches zero
    public readonly double HlMax;             // peak highlight opacity fraction
    public readonly double ShMax;             // peak shadow   opacity fraction
    public readonly double EdgeFadeFraction;  // fraction of vertical edge with secondary effect

    // ── Corner atlas (owned; built eagerly at construction) ──────────────────
    // 2N×2N sprite sheet with pre-rendered corners and baked-in 3-D effects.
    // Layout: TL=(0,0)  TR=(N,0)  BL=(0,N)  BR=(N,N)  where N=BezelThickness.
    private readonly Bitmap _cornerAtlas;

    public BezelRenderer(
        Color  bezelColor,
        int    bezelThickness,
        int    threeDEffectDepth,
        double fadeStart,
        double fadeEnd,
        double hlMax,
        double shMax,
        double edgeFadeFraction)
    {
        BezelColor        = bezelColor;
        BezelThickness    = bezelThickness;
        ThreeDEffectDepth = threeDEffectDepth;
        FadeStart         = fadeStart;
        FadeEnd           = fadeEnd;
        HlMax             = hlMax;
        ShMax             = shMax;
        EdgeFadeFraction  = edgeFadeFraction;

        _cornerAtlas = BezelGraphics.GetCornerTemplates(
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
        var N = BezelThickness;

        // ── Straight edge flat fills ──────────────────────────────────────────
        // Fill the four straight edge strips (between corners) with flat BezelColor.
        // The 3-D effects are overlaid by DrawBezelEdges; corners are drawn last.
        var savedMode = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.None;
        using (var flatBrush = new SolidBrush(BezelColor))
        {
            g.FillRectangle(flatBrush, x + N, y,              width - 2 * N, N);  // top
            g.FillRectangle(flatBrush, x + N, y + height - N, width - 2 * N, N);  // bottom
            g.FillRectangle(flatBrush, x,             y + N,  N,             height - 2 * N);  // left
            g.FillRectangle(flatBrush, x + width - N, y + N,  N,             height - 2 * N);  // right
        }
        g.SmoothingMode = savedMode;

        // ── Straight edge 3-D effects ─────────────────────────────────────────
        // Overlay highlight / shadow gradient strips on the outer and inner depth
        // layers of all four straight edge segments.
        BezelGraphics.DrawBezelEdges(
            g,
            x, y, width, height,
            BezelThickness, ThreeDEffectDepth,
            BezelColor, HlMax, ShMax, EdgeFadeFraction);

        // ── Corners (flat fill + 3-D effects baked in) ────────────────────────
        // Drawn last so the antialiased outer-edge pixels composite correctly
        // over whatever was drawn in the edge strips above.
        var corners = new[]
        {
            // (source region in atlas,      destination on target)
            (src: new Rectangle(0, 0, N, N), dest: new Rectangle(x,             y,              N, N)),  // TL
            (src: new Rectangle(N, 0, N, N), dest: new Rectangle(x + width - N, y,              N, N)),  // TR
            (src: new Rectangle(0, N, N, N), dest: new Rectangle(x,             y + height - N, N, N)),  // BL
            (src: new Rectangle(N, N, N, N), dest: new Rectangle(x + width - N, y + height - N, N, N)),  // BR
        };
        foreach (var (src, dest) in corners)
            g.DrawImage(_cornerAtlas, dest, src, GraphicsUnit.Pixel);
    }

    // ── Disposal ────────────────────────────────────────────────────────────

    public void Dispose()
    {
        _cornerAtlas.Dispose();
    }
}
