namespace FancyMouse.Drawing.Bezels;

/// <summary>
/// Models the cross-sectional surface geometry of a bezel ring, mapping a
/// position across the bezel to a surface normal angle, with p=0 representing
/// the *outer* edge of the bezel and p=1 representing the inner edge.
/// </summary>
/// <remarks>
/// The cross-section profile is a straight inclined ramp of width 'w' at each edge,
/// with a flat surface between them. Unlike <see cref="BezelProfileCurved"/>
/// the normal is constant across each ramp, so it steps (rather than sweeps)
/// between sections:
///
///   w   = ramp width
///   α   = π/2 - ramp angle (the ramp's inclination from horizontal)
///   p=0 => outer bezel edge
///   p=1 => inner bezel edge
///
///                           ______________
///                         /|              |\
///                       /  |              |  \
///                     /    |              |    \
///                   /      |              |      \
///                 /        |              |        \
///                +---------+--------------+---------+
///   position:    0         w             1-w        1
///   θ (normal):  |    α    |      π/2     |   π-α   |
///   cos(θ):      | +cos α  |       0      | -cos α  |
///
/// GetProfileNormal maps a position to a normal angle, and the shared helpers
/// in BezelProfile convert that angle to highlight / shadow intensities for
/// rendering, with positive values representing highlight effect intensity,
/// and negative values representing shadow effect intensity.
/// </remarks>
internal sealed class BezelProfileRamped : IBezelProfile
{
    /// <remarks>
    /// Convenience overload for callers working in pixels and degrees rather than
    /// <see cref="RampWidth"/> and <see cref="RampAngle"/> directly - equivalent to
    /// <c>new BezelProfileRamped(bezelDepth / (double)bezelWidth, rampAngleDegrees * Math.PI / 180.0)</c>.
    /// </remarks>
    /// <param name="bezelWidth">The full bezel ring width in pixels (outer bezel edge → inner bezel edge).</param>
    /// <param name="bezelDepth">The width in pixels of the ramp at each edge of the bezel.</param>
    /// <param name="rampAngleDegrees">The inclination of each ramp from horizontal, in degrees.</param>
    internal BezelProfileRamped(int bezelWidth, int bezelDepth, double rampAngleDegrees)
        : this(bezelDepth / (double)bezelWidth, rampAngleDegrees * Math.PI / 180.0)
    {
    }

    internal BezelProfileRamped(double rampWidth, double rampAngle)
    {
        if (rampWidth < 0.0 || rampWidth > 0.5)
        {
            throw new ArgumentOutOfRangeException(nameof(rampWidth), rampWidth, $"{nameof(rampWidth)} must be between 0 and 0.5 inclusive.");
        }

        if (rampAngle < 0.0 || rampAngle > Math.PI / 2.0)
        {
            throw new ArgumentOutOfRangeException(nameof(rampAngle), rampAngle, $"{nameof(rampAngle)} must be between 0 and π/2 radians inclusive.");
        }

        this.RampWidth = rampWidth;
        this.RampAngle = rampAngle;
    }

    /// <summary>
    /// Gets the width of the ramps at the start and end of the profile,
    /// as a proportion of the full profile width. e.g. 0.25 means 25% of
    /// the profile width.
    /// </summary>
    /// <remarks>
    /// Notable values:
    ///
    ///     * 0.00 - a flat bezel with no raised profile, and no lighting effect
    ///              as a result
    ///
    ///     * 0.25 - each ramp occupies a quarter of the profile width, with the
    ///              flat portion in the middle occupying half the profile width
    ///
    ///     * 0.50 - the ramps meet in the middle and form a peaked (roof-shaped)
    ///              profile, which is the largest allowed value (any larger and the
    ///              ramps would overlap in the middle of the profile)
    /// </remarks>
    private double RampWidth
    {
        get;
    }

    /// <summary>
    /// Gets the inclination of each ramp from horizontal, in radians.
    /// </summary>
    /// <remarks>
    /// Notable values:
    ///
    ///     * 0   - the ramps lie flat, so there's no lighting effect
    ///
    ///     * π/2 - the ramps are vertical walls, facing directly toward (outer)
    ///             and away from (inner) the light source for full highlight / shadow
    /// </remarks>
    private double RampAngle
    {
        get;
    }

    /// <summary>
    /// Returns the surface normal angle in radians at <paramref name="position"/>
    /// across the bezel (0 = outer bezel edge, 1 = inner bezel edge).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="position"/> is less than 0 or greater than 1.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="position"/> is NaN.
    /// </exception>
    public double GetProfileNormal(double position)
    {
        var rampWidth = this.RampWidth;
        return position switch
        {
            // before the outer bezel edge
            _ when position < 0.0 =>
                throw new ArgumentOutOfRangeException(nameof(position), position, "position must be between 0 and 1 inclusive."),

            // on outer ramp
            _ when position < rampWidth =>
                (Math.PI / 2.0) - this.RampAngle,

            // flat central section
            // (must match position '1' when rampWidth == 0, and keeps both ramps the same width)
            _ when position <= 1.0 - rampWidth =>
                Math.PI / 2.0,

            // on inner ramp
            _ when position <= 1.0 =>
                (Math.PI / 2.0) + this.RampAngle,

            // past the inner bezel edge
            _ when position > 1.0 =>
                throw new ArgumentOutOfRangeException(nameof(position), position, "position must be between 0 and 1 inclusive."),

            // only NaN gets here: it fails every comparison above
            _ =>
                throw new ArgumentException("position must be a number.", nameof(position)),
        };
    }
}
