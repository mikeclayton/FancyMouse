namespace FancyMouse.Models.Drawing;

public sealed class BackgroundInfo
{
    public static readonly BackgroundInfo Empty = new(SystemColors.Control, SystemColors.Control);

    public BackgroundInfo(
        Color color1,
        Color color2)
    {
        this.Color1 = color1;
        this.Color2 = color2;
    }

    public Color Color1
    {
        get;
    }

    public Color Color2
    {
        get;
    }

    public override string ToString()
    {
        return "{" +
           $"{nameof(this.Color1)}={this.Color1}," +
           $"{nameof(this.Color2)}={this.Color2}" +
           "}";
    }
}
