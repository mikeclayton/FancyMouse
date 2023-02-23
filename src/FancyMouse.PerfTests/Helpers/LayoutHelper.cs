namespace FancyMouse.PerfTests.Helpers;

internal static class LayoutHelper
{
    /// <summary>
    /// Combines the specified regions and returns the smallest rectangle that contains them.
    /// </summary>
    /// <param name="regions">The regions to combine.</param>
    /// <returns>
    /// Returns the smallest rectangle that contains all the specified regions.
    /// </returns>
    public static Rectangle CombineRegions(IList<Rectangle> regions)
    {
        if (regions == null)
        {
            throw new ArgumentNullException(nameof(regions));
        }

        if (regions.Count == 0)
        {
            return Rectangle.Empty;
        }

        var combined = regions.Aggregate(
            seed: regions[0],
            func: Rectangle.Union);

        return combined;
    }

    /// <summary>
    /// Scale an object to fit inside the specified bounds while maintaining aspect ratio.
    /// </summary>
    public static double GetScalingRatio(Size obj, Size bounds)
    {
        if (bounds.Width == 0 || bounds.Height == 0)
        {
            return 0;
        }

        var widthRatio = (double)bounds.Width / obj.Width;
        var heightRatio = (double)bounds.Height / obj.Height;
        var scalingRatio = Math.Min(widthRatio, heightRatio);

        return scalingRatio;
    }
}
