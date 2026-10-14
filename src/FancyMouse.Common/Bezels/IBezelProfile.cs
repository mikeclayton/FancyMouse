namespace FancyMouse.Common.Bezels;

/// <summary>
/// Defines the cross-sectional surface geometry of a screen bezel.
/// Implementations map a position across the profile to a surface normal angle
/// that the shared lighting helpers in <see cref="BezelProfile"/> convert
/// to highlight / shadow intensities.
/// </summary>
/// <remarks>
/// The profile function allows different physical bezel shapes to be rendered
/// just by implementing a function that returns the normals across the
/// cross-section. The rendering pipeline uses this function to calculate the
/// lighting and shading effects at each rasterized pixel in the output image.
///
/// For example a profile function might describe the normals on a semi-circular
/// cross-section (where 0 represents the outer edge, and 1 represents the
/// inner edge):
///
///       _.-""""""-._
///     .'            '.
///    /                \
///   |                  |
///   +------------------+
///   0                  1
///
/// or a raised flat profile with ramped edges (<see cref="BezelProfileRamped"/>):
///
///      ______________
///     /              \
///    /                \
///   /                  \
///   +------------------+
///   0                  1
///
/// or a raised flat profile with curved edges (<see cref="BezelProfileCurved"/>):
///
///      _------------_
///    /                \
///   |                  |
///   +------------------+
///   0                  1
///
/// or an arbitrarily complex cross section:
///
///    .--.   .--.   .--.
///   /    \_/    \_/    \
///   |                  |
///   +------------------+
///   0                  1
///
/// The angle of the normal at each position is used to calculate the
/// intensity of the highlight or shading effect at that position (but
/// note that it only affects the highlight and the shading on the point
/// itself - no shadows are cast by peaks in the profile onto other points)
/// </remarks>
internal interface IBezelProfile
{
    /// <summary>
    /// Returns the surface normal angle in radians at <paramref name="position"/>
    /// of the way from the outer arc boundary to the content boundary
    /// (0 = outer arc edge, 1 = content boundary).
    ///
    /// The angle is measured relative to the light source direction, not the
    /// viewer: implementations must return 0 where the local surface normal
    /// faces the light directly (full highlight), π/2 where it faces sideways
    /// (no effect), and π where it faces directly away from the light (full
    /// shadow). <see cref="BezelProfile.GetLightingEffectIntensity"/> applies Lambert's
    /// cosine law to this angle and relies on every implementation honoring
    /// this convention.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="position"/> is less than 0 or greater than 1.
    /// </exception>
    double GetProfileNormal(double position);
}
