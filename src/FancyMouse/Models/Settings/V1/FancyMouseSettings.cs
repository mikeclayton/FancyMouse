using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings.V1;

public sealed class FancyMouseSettings
{
    public FancyMouseSettings(
        string? hotkey,
        string? previewSize)
    {
        this.Hotkey = hotkey;
        this.PreviewSize = previewSize;
    }

    [JsonPropertyName("hotkey")]
    public string? Hotkey
    {
        get;
    }

    [JsonPropertyName("preview")]
    public string? PreviewSize
    {
        get;
    }
}
