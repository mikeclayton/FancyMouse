using System.Text.Json.Serialization;

namespace FancyMouse.Internal.Models.Settings.V2;

/// <summary>
/// Represents the border style for a drawing object.
/// </summary>
internal sealed class BorderStyleSettings
{
    public BorderStyleSettings(string? color, decimal? width, decimal? depth)
    {
        this.Color = color;
        this.Width = width;
        this.Depth = depth;
    }

    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color
    {
        get;
    }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Width
    {
        get;
    }

    /// <summary>
    /// Gets the "depth" of the 3d highlight and shadow effect on the border.
    /// </summary>
    [JsonPropertyName("depth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Depth
    {
        get;
    }

    public override string ToString()
    {
        return "{" +
           $"{nameof(this.Color)}={this.Color}," +
           $"{nameof(this.Width)}={this.Width}," +
           $"{nameof(this.Depth)}={this.Depth}" +
           "}";
    }
}
