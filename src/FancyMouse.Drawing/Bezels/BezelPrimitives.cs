using System.Drawing;
using System.Drawing.Drawing2D;

namespace FancyMouse.Drawing.Bezels;

internal static class BezelPrimitives
{
    /// <summary>
    /// Returns the 3-D effect weight at angular distance <paramref name="theta"/> degrees
    /// from a straight bezel edge. Full strength from 0° to <paramref name="fadeStart"/>,
    /// cosine rolloff from <paramref name="fadeStart"/> to <paramref name="fadeEnd"/>,
    /// zero beyond.
    /// </summary>
    internal static double CornerWeight(double theta, double fadeStartDegrees, double fadeEndDegrees)
        => CosineEase(theta, fadeStartDegrees, fadeEndDegrees, 1.0, 0.0);

    /// <summary>
    /// Implements a cosine easing function that can be used creating a
    /// smooth transition between two values within an interval.
    /// <code>
    ///  +-----+---+-----+
    ///   -----.   .       - start value
    ///        .\  .
    ///        . \ .
    ///        .  \.
    ///        .   .-----  - end value
    ///  +-----+---+-----+
    ///        ^   ^
    ///        |   interval end
    ///        interval start
    /// </code>
    ///
    /// </summary>
    /// <returns>
    /// if x is less than intervalStart, returns startValue.
    /// if x is more than intervalEnd,returns endValue.
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

        var intervalWidth = intervalEnd - intervalStart;
        if (intervalWidth <= 0)
        {
            return startValue;
        }

        var t = (x - intervalStart) / intervalWidth;
        var weight = 0.5 * (1.0 - Math.Cos(Math.PI * t));

        var delta = endValue - startValue;
        return startValue + (delta * weight);
    }

    /// <summary>
    /// Returns the GDI screen angle in degrees for a pixel offset (dx, dy) from an arc centre.
    /// 0° = rightward, increasing clockwise. Result is always in [0, 360).
    /// </summary>
    internal static double GdiAngle(int dx, int dy)
    {
        var a = Math.Atan2(dy, dx) * (180.0 / Math.PI);
        return a < 0 ? a + 360.0 : a;
    }

    /// <summary>
    /// Inverse of <see cref="MidpointFade"/>: 0.0 at the edge junctions, 1.0 at the
    /// 45° corner midpoint.  Used on TL / BR corners to add a secondary-effect peak
    /// halfway round the double-highlight / double-shadow arc.
    /// </summary>
    internal static double MidpointPeak(double theta)
        => MidpointFade(Math.Abs(theta - 45.0));

    /// <summary>
    /// Fades 1.0 → 0.0 from a straight edge (θ = 0°) to the 45° corner midpoint.
    /// Used on TR / BL corners where highlight meets shadow; both contributions fade
    /// to zero at 45° so the bezel colour is clean at the diagonal.
    /// </summary>
    internal static double MidpointFade(double theta)
        => CosineEase(theta, 0.0, 45.0, 1.0, 0.0);

    /// <summary>
    /// Blends highlight (<paramref name="hl"/>) and shadow (<paramref name="sh"/>) multipliers
    /// onto <paramref name="baseColor"/>.  Actual intensity is capped at
    /// <paramref name="hlMax"/> / <paramref name="shMax"/> so the effect stays subtle.
    /// </summary>
    internal static Color ApplyEffect(
        double hl,
        double sh,
        Color baseColor,
        double hlMax,
        double shMax)
    {
        var ha = Math.Min(1.0, hl * hlMax);
        var sa = Math.Min(1.0, sh * shMax);
        var r = (baseColor.R + (ha * (255 - baseColor.R))) * (1 - sa);
        var g = (baseColor.G + (ha * (255 - baseColor.G))) * (1 - sa);
        var b = (baseColor.B + (ha * (255 - baseColor.B))) * (1 - sa);
        return Color.FromArgb(
            255,
            (int)Math.Clamp(r, 0, 255),
            (int)Math.Clamp(g, 0, 255),
            (int)Math.Clamp(b, 0, 255));
    }

    internal static GraphicsPath GetRoundedRectanglePath(int x, int y, int w, int h, int r)
    {
        var path = new GraphicsPath();
        if (r > 0)
        {
            var d = 2 * r;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
        }
        else
        {
            path.AddRectangle(new Rectangle(x, y, w, h));
        }

        path.CloseFigure();
        return path;
    }
}
