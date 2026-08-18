using System.Drawing;

namespace FancyMouse.Common.Bezels;

internal static class BezelPrimitives
{
    /// <summary>
    /// Implements a cosine easing function that can be used to create a
    /// smooth transition between two values on a continuous axis within
    /// an interval.
    /// <code>
    ///  +-----+-----+-----+
    ///   -----._    .       - start value
    ///        . \   .
    ///        .  \  .
    ///        .   \ .
    ///        .    -._____  _ end value
    ///  +-----+-----+-----+
    ///        ^   ^
    ///        |   easing interval end
    ///        easing interval start
    /// </code>
    ///
    /// </summary>
    /// <returns>
    /// if x <= easingIntervalStart, returns startValue.
    /// if x >= easingIntervalEnd, returns endValue.
    /// otherwise returns an eased value between startValue and endValue
    /// </returns>
    internal static double CosineEase(
        double x,
        double intervalStart,
        double intervalEnd,
        double startValue,
        double endValue)
    {
        if (x <= intervalStart)
        {
            return startValue;
        }

        if (x >= intervalEnd)
        {
            return endValue;
        }

        var easingIntervalWidth = intervalEnd - intervalStart;

        var t = (x - intervalStart) / easingIntervalWidth;
        var weight = 0.5 * (1.0 - Math.Cos(Math.PI * t));

        var valueDelta = endValue - startValue;
        return startValue + (valueDelta * weight);
    }

    /// <summary>
    /// Calculates the intensity of a gradient fill at a given angle around
    /// a bezel corner using the following rules:
    ///
    /// * full intensity from 0° to <paramref name="fadeStartDegrees"/>
    /// * cosine rolloff from <paramref name="fadeStartDegrees"/> to <paramref name="fadeEndDegrees"/>
    /// * zero intensity beyond <paramref name="fadeEndDegrees"/>
    /// </summary>
    /// <param name="theta">Angle in degrees, measured from the nearest straight edge.</param>
    internal static double CornerEffectWeight(double theta, double fadeStartDegrees, double fadeEndDegrees)
        => CosineEase(theta, fadeStartDegrees, fadeEndDegrees, 1.0, 0.0);

    /// <summary>
    /// Returns the GDI screen angle in degrees for a pixel offset (dx, dy) from an arc centre.
    /// 0° = rightward, increasing clockwise. Result is always in [0, 360).
    /// </summary>
    internal static double GdiAngle(int dx, int dy)
    {
        var angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
        return angle < 0
            ? angle + 360.0
            : angle;
    }

    /// <summary>
    /// Returns 0.0 at the straight-edge junctions (θ = 0° and θ = 90°), rising to
    /// 1.0 at the 45° corner midpoint. Used on TL / BR corners to add a
    /// secondary-effect peak halfway round the double-highlight / double-shadow arc.
    ///
    /// Uses a half-sine curve (sin(θ × π/90)) for a smooth, continuous transition
    /// that is exactly 0 at both endpoints and avoids visible seams where the corner
    /// arc meets the straight edges.
    /// </summary>
    /// <param name="theta">Angle in degrees, measured from the nearest straight edge.</param>
    internal static double MidpointPeak(double theta)
        => Math.Sin(theta * Math.PI / 90.0);

    /// <summary>
    /// Fades 1.0 → 0.0 from a straight edge (θ = 0°) to the 45° corner midpoint.
    /// Used on TR / BL corners where highlight meets shadow; both contributions fade
    /// to zero at 45° so the bezel colour is clean at the diagonal.
    /// </summary>
    /// <param name="theta">Angle in degrees, measured from the nearest straight edge.</param>
    internal static double MidpointFade(double theta)
        => CosineEase(theta, 0.0, 45.0, 1.0, 0.0);

    /// <summary>
    /// Blends highlight (<paramref name="hl"/>) and shadow (<paramref name="sh"/>) multipliers
    /// onto <paramref name="baseColor"/>.  Actual intensity is capped at
    /// <paramref name="hlMax"/> or <paramref name="shMax"/> so the effect stays subtle.
    /// </summary>
    internal static Color ApplyEffect(
        double highlightLevel,
        double shadowLevel,
        Color baseColor,
        double hlMax,
        double shMax)
    {
        var highlightAlpha = Math.Min(1.0, highlightLevel * hlMax);
        var shadowAlpha = Math.Min(1.0, shadowLevel * shMax);
        var r = (baseColor.R + (highlightAlpha * (255 - baseColor.R))) * (1 - shadowAlpha);
        var g = (baseColor.G + (highlightAlpha * (255 - baseColor.G))) * (1 - shadowAlpha);
        var b = (baseColor.B + (highlightAlpha * (255 - baseColor.B))) * (1 - shadowAlpha);
        return Color.FromArgb(
            255, // we don't calculate alpha, just rgb
            (int)Math.Clamp(r, 0, 255),
            (int)Math.Clamp(g, 0, 255),
            (int)Math.Clamp(b, 0, 255));
    }
}
