namespace FancyMouse.Models.Settings;

public sealed class CanvasSizeSettings
{
    public CanvasSizeSettings(decimal width, decimal height)
    {
        this.Width = width;
        this.Height = height;
    }

    public decimal Width
    {
        get;
    }

    public decimal Height
    {
        get;
    }

    public override string ToString()
    {
        return "{" +
            $"{nameof(this.Width)}={this.Width}," +
            $"{nameof(this.Height)}={this.Height}" +
            "}";
    }
}
