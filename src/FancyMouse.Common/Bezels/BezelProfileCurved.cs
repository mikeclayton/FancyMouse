namespace FancyMouse.Common.Bezels;

/// <summary>
/// Models the cross-sectional surface geometry of a bezel ring, mapping a
/// position across the bezel to a surface normal angle, with p=0 representing
/// the *outer* edge of the bezel and p=1 representing the inner edge.
/// </summary>
/// <remarks>
/// The cross-section profile is a quadrant of radius 'r' at each edge,
/// with a flat surface between them:
///
///   r   = curve radius
///   p=0 => outer bezel edge
///   p=1 => inner bezel edge
///
///                       _.--""""""""""""""""--._
///                    .-'    |              |    '-.
///                  .'       |              |       '.
///                 /         |              |         \
///                |          |              |          |
///                +----------+--------------+----------+
///   position:    0          r             1-r         1
///   θ (normal):  0         π/2            π/2         π
///   cos(θ):     +1          0              0         -1
///
/// GetProfileNormal maps a position to a normal angle, and the shared helpers
/// in BezelProfile convert that angle to highlight / shadow intensities for
/// rendering, with positive values representing highlight effect intensity,
/// and negative values representing shadow effect intensity.
/// </remarks>
internal sealed class BezelProfileCurved : IBezelProfile
{
    /// <remarks>
    /// Convenience overload for callers working in pixels rather than <see cref="CurveRadius"/>
    /// directly - equivalent to <c>new BezelProfileCurved(bezelDepth / (double)bezelWidth)</c>.
    /// </remarks>
    /// <param name="bezelWidth">The full bezel ring width in pixels (outer bezel edge → inner bezel edge).</param>
    /// <param name="bezelDepth">The radius in pixels of the curve at each edge of the bezel.</param>
    internal BezelProfileCurved(int bezelWidth, int bezelDepth)
        : this(bezelDepth / (double)bezelWidth)
    {
    }

    internal BezelProfileCurved(double curveRadius)
    {
        if (curveRadius < 0.0 || curveRadius > 0.5)
        {
            throw new ArgumentOutOfRangeException(nameof(curveRadius), curveRadius, $"{nameof(curveRadius)} must be between 0 and 0.5 inclusive.");
        }

        this.CurveRadius = curveRadius;
    }

    /// <summary>
    /// Gets the radius of the curves at the start and end of the profile,
    /// as a proportion of the full profile width. e.g. 0.25 means 25% of
    /// the profile width.
    /// </summary>
    /// <remarks>
    /// Notable values:
    ///
    ///     * 0.00 - a flat bezel with no raised profile, and no lighting effect
    ///              as a result
    ///
    ///     * 0.25 - each curve occupies a quarter of the profile width, with the
    ///              flat portion in the middle occupying half the profile width
    ///
    ///     * 0.50 - the curves meet in the middle and form a perfect semi-circular
    ///              profile, which is the largest allowed value (any larger and the
    ///              curves would overlap in the middle of the profile)
    /// </remarks>
    private double CurveRadius
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
        var curveRadius = this.CurveRadius;
        return position switch
        {
            // before the outer bezel edge
            _ when position < 0.0 =>
                throw new ArgumentOutOfRangeException(nameof(position), position, "position must be between 0 and 1 inclusive."),

            // on outer curve
            _ when position < curveRadius =>
                BezelProfileCurved.GetCurveNormal(position / curveRadius),

            // flat central section
            // (must match position '1' when curveRadius == 0)
            _ when position <= 1.0 - curveRadius =>
                Math.PI / 2.0,

            // on inner curve
            _ when position <= 1.0 =>
                Math.PI - BezelProfileCurved.GetCurveNormal((1.0 - position) / curveRadius),

            // past the inner bezel edge
            _ when position > 1.0 =>
                throw new ArgumentOutOfRangeException(nameof(position), position, "position must be between 0 and 1 inclusive."),

            // only NaN gets here: it fails every comparison above
            _ =>
                throw new ArgumentException("position must be a number.", nameof(position)),
        };
    }

    /// <summary>
    /// Returns the surface normal angle in radians at a proportion <paramref name="t"/>
    /// of the way across a "top left" quarter-circle curve.
    /// </summary>
    private static double GetCurveNormal(double t)
    {
        return Math.Acos(1.0 - t);
    }
}
