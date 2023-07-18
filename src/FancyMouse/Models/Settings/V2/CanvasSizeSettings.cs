namespace FancyMouse.Models.Settings.V2;

public sealed class CanvasSizeSettings
{
    public CanvasSizeSettings(
        int width,
        int height)
    {
        this.Width = width;
        this.Height = height;
    }

    public int Width
    {
        get;
    }

    public int Height
    {
        get;
    }
}
