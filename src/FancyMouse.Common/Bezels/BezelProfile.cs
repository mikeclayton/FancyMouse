using System.Drawing;

namespace FancyMouse.Common.Bezels;

/// <summary>
/// Lighting helpers shared by all <see cref="IBezelProfile"/> implementations.
/// These methods are profile-agnostic: they delegate the geometry question
/// ("what is the surface normal here?") to the profile and handle the rest.
/// </summary>
internal static class BezelProfile
{
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
    internal static double GetEffectIntensity(double normalAngle)
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
    internal static double GetEdgeNormal(this IBezelProfile profile, int d2)
        => profile.GetProfileNormal(d2);

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
    internal static double GetCornerNormal(this IBezelProfile profile, int n, Point originOffset)
    {
        var r = Math.Sqrt(
            (double)(originOffset.X * originOffset.X) +
            (double)(originOffset.Y * originOffset.Y));
        var position = (double)n - r;
        if (originOffset.X > 0)
        {
            position = Math.Min(position, (n - 1) - (double)originOffset.X);
        }

        if (originOffset.Y > 0)
        {
            position = Math.Min(position, (n - 1) - (double)originOffset.Y);
        }

        return profile.GetProfileNormal(position);
    }

    // ── Convenience ──────────────────────────────────────────────────────────

    /// <summary>Lighting intensity for a straight-edge pixel at depth <paramref name="d2"/>.</summary>
    internal static double GetEdgeIntensity(this IBezelProfile profile, int d2)
        => GetEffectIntensity(profile.GetEdgeNormal(d2));

    /// <summary>
    /// Signed lighting intensity for a corner pixel at <paramref name="originOffset"/>.
    /// Positive = highlight (outer arc), negative = shadow (inner arc), ~0 = flat zone.
    /// </summary>
    internal static double GetCornerIntensity(this IBezelProfile profile, int n, Point originOffset)
        => GetEffectIntensity(profile.GetCornerNormal(n, originOffset));
}
