namespace FancyMouse.Extensions;

internal static class SizeExtensions
{

    public static Size ScaleToFit(this Size obj, Size bounds)
    {

        var widthRatio = (double)obj.Width / bounds.Width;
        var heightRatio = (double)obj.Height / bounds.Height;

        var scaledSize = (widthRatio > heightRatio)
            ? new Size(
                bounds.Width,
                (int)(obj.Height / widthRatio)
            )
            : new Size(
                (int)(obj.Width / heightRatio),
                bounds.Height
            );

        return scaledSize;

    }

}
