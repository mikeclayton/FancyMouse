using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings.V2;

/// <summary>
/// Represents the background fill style for a drawing object.
/// </summary>
public sealed class BackgroundStyleSettings
{
    public BackgroundStyleSettings(
        string? color1,
        string? color2)
    {
        this.Color1 = color1;
        this.Color2 = color2;
    }

    [JsonPropertyName("color1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color1
    {
        get;
    }

    [JsonPropertyName("color2")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Color2
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
