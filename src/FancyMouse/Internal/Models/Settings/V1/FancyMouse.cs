using System.Text.Json.Serialization;

namespace FancyMouse.Internal.Models.Settings.V1;

/// <summary>
/// Represents the "fancymouse"' node in the V1 config file
/// </summary>
internal sealed class FancyMouse
{
    public FancyMouse(
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
