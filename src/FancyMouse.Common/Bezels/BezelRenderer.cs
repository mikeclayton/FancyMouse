using System.Drawing;
using System.Drawing.Drawing2D;

using FancyMouse.Models.Styles;

namespace FancyMouse.Common.Bezels;

/// <summary>
/// A drawing utility that can draw borders and bezels using a
/// single fixed style that is provided at construction. This
/// allows the instance to make optimisations by caching re-usable
/// assets that are locked to the style settings. To draw borders
/// with a different style, construct a separate BezelRenderer
/// instance.
/// </summary>
public sealed class BezelRenderer : IDisposable
{
    public BezelRenderer(BorderStyle bezelStyle)
    {
        this.BezelStyle = BezelRenderer.ClampDepth(bezelStyle ?? throw new ArgumentNullException(nameof(bezelStyle)));
        this.Profile = new BezelProfileCurved((int)this.BezelStyle.Left, (int)this.BezelStyle.Depth);
        _cornerAtlas = CornerTemplates.GetCornerTemplates(
            (int)this.BezelStyle.Left,
            this.BezelStyle.Color ?? Color.Transparent,
            this.Profile);
    }

    private BorderStyle BezelStyle
    {
        get;
    }

    // the bezel profile is scale-independent (a proportion, not a pixel count - see
    // IBezelProfile.GetProfileNormal), so one instance is valid at both the 1× scale
    // the edges render at and the 2× supersampled scale the corner atlas renders at -
    // constructed once here and threaded through both rendering paths.
    private IBezelProfile Profile
    {
        get;
    }

    // ── Corner atlas (owned; built eagerly at construction) ──────────────────
    // 2N×2N sprite sheet with pre-rendered corners and baked-in 3-D effects.
    // Layout: TL=(0,0)  TR=(N,0)  BL=(0,N)  BR=(N,N)  where N=Thickness.
    private readonly Bitmap _cornerAtlas;

    /// <summary>
    /// Returns a border style with the 3d effect depth limited to half the border thickness.
    /// This prevents the "light" and "shadow" effects overlapping (or overwriting each other)
    /// in the middle of the border.
    /// </summary>
    internal static BorderStyle ClampDepth(BorderStyle borderStyle)
    {
        var minThickness = Math.Min(
            Math.Min(borderStyle.Left, borderStyle.Top),
            Math.Min(borderStyle.Right, borderStyle.Bottom));
        var maxDepth = minThickness / 2;
        return (borderStyle.Depth <= maxDepth)
            ? borderStyle
            : borderStyle.WithDepth(maxDepth);
    }

    /// <summary>
    /// Draws a bezel ring with the full 3-D highlight/shadow corner effect.
    ///
    /// Light source is top-left:
    ///   TL — double highlight, peak at 45°
    ///   BR — double shadow,    peak at 45°
    ///   TR — highlight (top) meets shadow (right), both fade at 45°
    ///   BL — shadow (bottom) meets highlight (left), both fade at 45°
    ///   Inner arc effects are reversed; BR inner highlight is halved.
    /// </summary>
    public void DrawBezel(Graphics g, int x, int y, int width, int height)
    {
        var bezelWidth = (int)this.BezelStyle.Left;

        // ── Straight edge fills + 3-D effects ────────────────────────────────
        // Fills all four strips with flat BezelColor then overlays highlight /
        // shadow gradient effects on the outer and inner depth layers.
        BezelGraphics.DrawBezelEdges(
            g,
            x,
            y,
            width,
            height,
            bezelWidth,
            this.BezelStyle.Color ?? Color.Transparent,
            this.Profile);

        // ── Corners (flat fill + 3-D effects baked in) ────────────────────────
        // Drawn last so the antialiased outer-edge pixels composite correctly
        // over whatever was drawn in the edge strips above.
        //
        // NearestNeighbor + PixelOffsetMode.Half ensures an exact 1:1 pixel copy.
        // With Half mode the sample point for dest pixel i lands at exactly i,
        // so NearestNeighbor snaps to the correct source pixel without bilinear
        // blurring. Default bilinear samples at i+0.5, shifting the arc edge ~0.5 px
        // inward and creating a visible gap between the corner arc and the edge strips.
        var n = bezelWidth;
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

    public void Dispose()
    {
        _cornerAtlas.Dispose();
    }
}
