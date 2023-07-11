namespace FancyMouse.Models.Settings.V2;

/// <summary>
/// Represents the margin style for a drawing object.
/// </summary>
public sealed class MarginStyleSettings
{
    public MarginStyleSettings(decimal width)
    {
        this.Width = width;
    }

    public decimal Width
    {
        get;
    }

    public override string ToString()
    {
        return "{" +
            $"{nameof(this.Width)}={this.Width}" +
            "}";
    }
}
