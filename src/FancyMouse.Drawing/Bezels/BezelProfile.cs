using System.Drawing;

namespace FancyMouse.Drawing.Bezels;

// ─────────────────────────────────────────────────────────────────────────────
// BezelProfile
//
// Models the cross-sectional surface geometry of a bezel ring, mapping pixel
// positions to surface normal angles and then to lighting intensities.
//
// Cross-section layout (position 0 = outer arc edge, n = content boundary):
//
//   position:  0      d           n-d      n
//              │      │            │       │
//   θ (normal):0 ──→ π/2 ──────── π/2 ──→ π
//   cos(θ):   +1 ──→  0  ──────── 0  ──→ -1
//              │outer ring│  flat  │inner ring│
//              │(highlight)│ (none) │(shadow) │
//
// GetProfileNormal is the core function.  GetEffectIntensity converts a normal
// angle to a signed intensity in [-1, +1] (Lambert's cosine law):
//   +1 = surface faces the light (outer arc edge, full highlight)
//    0 = surface faces sideways  (flat zone, no effect)
//   -1 = surface faces away      (inner arc edge, full shadow)
//
// GetEdgeNormal / GetCornerNormal compute the normal for a specific pixel, and
// GetEdge / GetCornerIntensity convenience methods compose those two steps.
//
// Construct locally at the point of use and discard — no caching is needed.
// ─────────────────────────────────────────────────────────────────────────────
internal sealed class BezelProfile
{
    private readonly int _n; // bezel ring pixel width (outer arc → content boundary)
    private readonly int _d; // 3D effect ring depth in pixels on each side

    internal BezelProfile(int n, int d)
    {
        _n = n;
        _d = d;
    }

    // ── Core profile ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the surface normal angle in radians at <paramref name="position"/>
    /// pixels from the outer arc boundary (0 = outer arc edge, <c>n</c> = content boundary).
    ///
    /// 0   — outer arc edge: surface faces the light source directly (full highlight).
    /// π/2 — flat zone junction: surface faces sideways (no effect).
    /// π   — content boundary: surface faces away from the light source (full shadow).
    /// </summary>
    internal double GetProfileNormal(double position)
    {
        if (position <= 0.0)
        {
            return 0.0; // at or beyond outer arc edge
        }

        if (position < _d)
        {
            return (position / _d) * (Math.PI / 2.0); // outer effect ring: 0 → π/2
        }

        if (position < _n - _d)
        {
            return Math.PI / 2.0; // flat zone — surface faces sideways
        }

        if (position < _n)
        {
            return Math.PI - (((_n - position) / _d) * (Math.PI / 2.0)); // inner effect ring: π/2 → π
        }

        return Math.PI; // at or beyond content boundary
    }

    // ── Lighting model ───────────────────────────────────────────────────────

    /// <summary>
    /// Converts a surface normal angle to a signed lighting intensity in [-1, +1]
    /// using Lambert's cosine law: <c>intensity = cos(normalAngle)</c>.
    ///
    /// +1.0 at normalAngle = 0   (outer arc edge — full highlight).
    ///  0.0 at normalAngle = π/2 (flat zone — no effect).
    /// -1.0 at normalAngle = π   (inner arc edge — full shadow).
    ///
    /// Use <c>Math.Abs</c> for magnitude; the sign distinguishes highlight
    /// (positive) from shadow (negative).
    /// </summary>
    internal double GetEffectIntensity(double normalAngle)
        => Math.Cos(normalAngle);

    // ── Normal-angle helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Returns the surface normal angle for a straight-edge pixel at depth
    /// <paramref name="d2"/> from the active edge of the effect ring.
    ///
    /// d2 = 0 is the active edge (outer arc boundary for the outer ring;
    /// content boundary for the inner ring).  Both rings use the same formula
    /// because d2 always measures distance from the active edge.
    /// </summary>
    internal double GetEdgeNormal(int d2)
        => GetProfileNormal(d2);

    /// <summary>
    /// Returns the surface normal angle for a corner pixel at
    /// <paramref name="originOffset"/> from the corner arc centre.
    /// Radial distance r maps to profile position <c>n − r</c>, clamped so that
    /// pixels at the right / bottom tile edges agree with the adjacent edge strips.
    ///
    /// At the right and bottom edges the arc boundary falls just outside the
    /// corner tile, so the tile's outermost pixels sit at radial depth ≈ 1 rather
    /// than 0.  Without clamping their effectMagnitude is cos(π/(2d)) ≈ 0.95
    /// instead of 1.0, while the adjacent straight-edge strip starts at d2 = 0
    /// (effectMagnitude = 1.0) — producing a visible seam.  Clamping to the
    /// strip's own perpendicular depth (n − 1 − X or n − 1 − Y) removes the
    /// discrepancy.  The clamp is inert for TL pixels (X ≤ 0, Y ≤ 0) because
    /// the arc boundary already lies at or beyond the tile corner there.
    /// </summary>
    internal double GetCornerNormal(Point originOffset)
    {
        var r = Math.Sqrt(
            (double)(originOffset.X * originOffset.X) +
            (double)(originOffset.Y * originOffset.Y));
        var position = (double)_n - r;
        if (originOffset.X > 0)
        {
            position = Math.Min(position, (_n - 1) - (double)originOffset.X);
        }

        if (originOffset.Y > 0)
        {
            position = Math.Min(position, (_n - 1) - (double)originOffset.Y);
        }

        return GetProfileNormal(position);
    }

    // ── Convenience ──────────────────────────────────────────────────────────

    /// <summary>Lighting intensity for a straight-edge pixel at depth <paramref name="d2"/>.</summary>
    internal double GetEdgeIntensity(int d2)
        => GetEffectIntensity(GetEdgeNormal(d2));

    /// <summary>
    /// Signed lighting intensity for a corner pixel at <paramref name="originOffset"/>.
    /// Positive = highlight (outer arc), negative = shadow (inner arc), ~0 = flat zone.
    /// </summary>
    internal double GetCornerIntensity(Point originOffset)
        => GetEffectIntensity(GetCornerNormal(originOffset));
}
