using System.Text.Json.Serialization;

namespace FancyMouse.Settings.V2;

public sealed class PreviewStyleSettings
{
    public PreviewStyleSettings(
        string? type,
        CanvasSizeSettings? canvasSize,
        CanvasStyleSettings? canvasStyle,
        ScreenStyleSettings? screenStyle)
    {
        this.Type = type;
        this.CanvasSize = canvasSize;
        this.CanvasStyle = canvasStyle;
        this.ScreenStyle = screenStyle;
    }

    /// <summary>
    /// Gets the raw <see cref="PreviewType"/> name - a plain string, same as <c>MouseJumpProperties</c>
    /// stores its own "preview_type" ("Custom"/"Compact"/"Bezelled"), not the enum itself, so an
    /// unrecognised or missing value degrades gracefully (see
    /// <see cref="SettingsConverterV2.GetActivePreviewStyle"/>) rather than failing to
    /// deserialize the whole file.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type
    {
        get;
    }

    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CanvasSizeSettings? CanvasSize
    {
        get;
    }

    [JsonPropertyName("canvas")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CanvasStyleSettings? CanvasStyle
    {
        get;
    }

    [JsonPropertyName("screenshot")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ScreenStyleSettings? ScreenStyle
    {
        get;
    }
}
