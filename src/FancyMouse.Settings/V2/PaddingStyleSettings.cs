using System.Text.Json.Serialization;

namespace FancyMouse.Settings.V2;

/// <summary>
/// Represents the margin style for a drawing object.
/// </summary>
public sealed class PaddingStyleSettings
{
    public PaddingStyleSettings(int? width)
    {
        this.Width = width;
    }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width
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
