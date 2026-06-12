namespace FancyMouse.Drawing.Bezels;

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
    /// </summary>
    double GetProfileNormal(double position);
}
