namespace FancyMouse.Helpers;

internal static class RectangleExtensions
{
    /// <summary>
    /// Combines the specified regions and returns the smallest rectangle that contains them.
    /// </summary>
    /// <param name="source">The rectangles to combine.</param>
    /// <returns>
    /// Returns the smallest rectangle that contains all the specified regions.
    /// </returns>
    public static Rectangle GetBoundingRectangle(this IList<Rectangle> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (source.Count == 0)
        {
            return Rectangle.Empty;
        }

        return source.Aggregate(
            seed: source[0],
            func: Rectangle.Union);
    }

    /// <summary>
    /// Returns the midpoint of the given region.
    /// </summary>
    public static Point GetMidpoint(this Rectangle source)
    {
        return new Point(
            (source.Left + source.Right) / 2,
            (source.Top + source.Bottom) / 2);
    }
}
