namespace FancyMouse.Models.Layout;

internal sealed class BorderLayout
{
    public BorderLayout(
        bool visible,
        int width,
        Color color)
    {
        this.Visible = visible;
        this.Width = width;
        this.Color = color;
    }

    public bool Visible
    {
        get;
    }

    public int Width
    {
        get;
    }

    public Color Color
    {
        get;
    }
}
