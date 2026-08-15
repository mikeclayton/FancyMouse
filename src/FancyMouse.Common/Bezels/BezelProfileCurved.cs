namespace FancyMouse.Common.Bezels;

// ─────────────────────────────────────────────────────────────────────────────
// BezelProfileCurved
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
// GetProfileNormal maps position to a normal angle; the shared helpers in
// BezelProfile convert that angle to highlight / shadow intensities.
//
// Construct locally at the point of use and discard — no caching is needed.
// ─────────────────────────────────────────────────────────────────────────────
internal sealed class BezelProfileCurved : IBezelProfile
{
    private readonly int _n; // bezel ring pixel width (outer arc → content boundary)
    private readonly int _d; // 3D effect ring depth in pixels on each side

    internal BezelProfileCurved(int n, int d)
    {
        _n = n;
        _d = d;
    }

    /// <summary>
    /// Returns the surface normal angle in radians at <paramref name="position"/>
    /// pixels from the outer arc boundary (0 = outer arc edge, <c>n</c> = content boundary).
    ///
    /// 0   — outer arc edge: surface faces the light source directly (full highlight).
    /// π/2 — flat zone junction: surface faces sideways (no effect).
    /// π   — content boundary: surface faces away from the light source (full shadow).
    /// </summary>
    public double GetProfileNormal(double position)
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
}
