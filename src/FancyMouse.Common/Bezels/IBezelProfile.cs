namespace FancyMouse.Common.Bezels;

/// <summary>
/// Defines the cross-sectional surface geometry of a bezel ring.
/// Implementations map a pixel depth position to a surface normal angle
/// that the shared lighting helpers in <see cref="BezelProfile"/> convert
/// to highlight / shadow intensities.
/// </summary>
internal interface IBezelProfile
{
    /// <summary>
    /// Returns the surface normal angle in radians at <paramref name="position"/>
    /// pixels from the outer arc boundary (0 = outer arc edge, n = content boundary).
    ///
    /// The angle is measured relative to the light source direction, not the
    /// viewer: implementations must return 0 where the local surface normal
    /// faces the light directly (full highlight), π/2 where it faces sideways
    /// (no effect), and π where it faces directly away from the light (full
    /// shadow). <see cref="BezelProfile.GetLightingEffectIntensity"/> applies Lambert's
    /// cosine law to this angle and relies on every implementation honoring
    /// this convention.
    /// </summary>
    double GetProfileNormal(double position);
}
