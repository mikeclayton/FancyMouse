using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings.V2;

public sealed class CanvasSizeSettings
{
    public CanvasSizeSettings(
        int? width,
        int? height)
    {
        this.Width = width;
        this.Height = height;
    }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width
    {
        get;
    }

    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Height
    {
        get;
    }
}
