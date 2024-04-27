using System.Text.Json.Serialization;

namespace FancyMouse.Internal.Models.Settings.V2;

/// <summary>
/// Represents the configuration file format to allow for easier
/// serialization / deserialization. This needs to be converted
/// into an AppSettings object for the main application to use.
/// </summary>
internal sealed class AppConfig
{
    public AppConfig(
        int? version,
        string? hotkey,
        PreviewStyleSettings? preview)
    {
        this.Version = version;
        this.Hotkey = hotkey;
        this.Preview = preview;
    }

    [JsonPropertyName("version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Version
    {
        get;
    }

    [JsonPropertyName("hotkey")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Hotkey
    {
        get;
    }

    [JsonPropertyName("preview")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PreviewStyleSettings? Preview
    {
        get;
    }
}
