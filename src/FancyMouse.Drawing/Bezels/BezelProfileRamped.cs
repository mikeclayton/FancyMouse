namespace FancyMouse.Drawing.Bezels;

// ─────────────────────────────────────────────────────────────────────────────
// BezelProfileRamped
//
// Models the cross-sectional surface geometry of a bezel ring as a flat
// inclined plane (chamfer / bevel), mapping pixel positions to a CONSTANT
// surface normal angle across each effect ring.
//
// Cross-section layout (position 0 = outer arc edge, n = content boundary):
//
//   position:  0      d           n-d      n
//              │      │            │       │
//   θ (normal):α ──── α ──────── π-α ──── π-α
//              │outer ring│  flat  │inner ring│
//              │(highlight)│ (none) │(shadow) │
//
//   where α = π/2 − rampAngle  (rampAngle is the inclination from horizontal)
//
// Unlike BezelProfileCurved (which sweeps 0 → π/2 through a quadrant arc),
// BezelProfileRamped holds a constant normal angle inside each ring, producing
// a uniform-intensity chamfer rather than a smooth gradient.  At 45° the
// intensity is cos(π/4) ≈ 0.707 throughout both effect rings.
//
// GetProfileNormal maps position to a normal angle; the shared helpers in
// BezelProfile convert that angle to highlight / shadow intensities.
//
// Construct locally at the point of use and discard — no caching is needed.
// ─────────────────────────────────────────────────────────────────────────────
internal sealed class BezelProfileRamped : IBezelProfile
{
    private readonly int _n;            // bezel ring pixel width (outer arc → content boundary)
    private readonly int _d;            // 3D effect ring depth in pixels on each side
    private readonly double _rampAngle; // inclination from horizontal, in radians

    internal BezelProfileRamped(int n, int d, double rampAngleDegrees)
    {
        _n = n;
        _d = d;
        _rampAngle = rampAngleDegrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Returns the surface normal angle in radians at <paramref name="position"/>
    /// pixels from the outer arc boundary (0 = outer arc edge, <c>n</c> = content boundary).
    ///
    /// The normal is constant within each effect ring (a flat inclined surface):
    ///   π/2 − rampAngle — outer ring (faces partly toward the light source).
    ///   π/2             — flat zone  (faces sideways; no effect).
    ///   π/2 + rampAngle — inner ring (faces partly away from the light source).
    /// </summary>
    public double GetProfileNormal(double position)
    {
        if (position < _d)
        {
            return (Math.PI / 2.0) - _rampAngle; // outer ring: constant ramp angle (highlight)
        }

        if (position < _n - _d)
        {
            return Math.PI / 2.0; // flat zone — surface faces sideways
        }

        return (Math.PI / 2.0) + _rampAngle; // inner ring: constant ramp angle (shadow)
    }
}
