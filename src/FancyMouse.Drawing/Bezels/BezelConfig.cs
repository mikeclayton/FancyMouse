namespace FancyMouse.Drawing.Bezels;

/// <summary>
/// Immutable style settings that control the 3-D lighting effect on a bezel.
/// These are independent of bezel size — the same config can be reused across
/// bezels of different thicknesses or colours.
/// </summary>
public sealed class BezelConfig
{
    public BezelConfig(
        double fadeStart,
        double fadeEnd,
        double highlightMax,
        double shadowMax,
        float edgeFadeFraction,
        double rampAngleDegrees)
    {
        FadeStart = fadeStart;
        FadeEnd = fadeEnd;
        HighlightMax = highlightMax;
        ShadowMax = shadowMax;
        EdgeFadeFraction = edgeFadeFraction;
        RampAngleDegrees = rampAngleDegrees;
    }

    /// <summary>
    /// Gets the angle in degrees from the nearest straight edge at which the corner
    /// highlight / shadow effect begins to fade. The effect is at full intensity
    /// between 0° and this value.
    /// </summary>
    public double FadeStart { get; }

    /// <summary>
    /// Gets the angle in degrees at which the corner highlight / shadow effect reaches
    /// zero intensity. The effect fades via a cosine curve between
    /// <see cref="FadeStart"/> and this value.
    /// </summary>
    public double FadeEnd { get; }

    /// <summary>
    /// Gets the peak opacity of the highlight (white) overlay, as a fraction of 255.
    /// 0.0 = no highlight; 1.0 = fully white at peak.
    /// </summary>
    public double HighlightMax { get; }

    /// <summary>
    /// Gets the peak opacity of the shadow (black) overlay, as a fraction of 255.
    /// 0.0 = no shadow; 1.0 = fully black at peak.
    /// </summary>
    public double ShadowMax { get; }

    /// <summary>
    /// Gets the fraction of a straight edge over which the corner highlight/shadow
    /// effect fades from full intensity to the flat base colour.
    /// 0.0 = effect drops immediately at the corner; 1.0 = effect runs the
    /// full length of the edge.
    /// </summary>
    public float EdgeFadeFraction { get; }

    /// <summary>
    /// Gets the inclination angle of the bezel ramp surface, in degrees.
    /// Used by <see cref="BezelProfileRamped"/> to set the constant surface normal
    /// across the outer and inner effect rings.
    /// 0° = horizontal surface (no shading); 90° = vertical wall (maximum shading).
    /// Typical value: 45° — gives cos(45°) ≈ 0.707 uniform intensity on each ring.
    /// </summary>
    public double RampAngleDegrees { get; }
}
