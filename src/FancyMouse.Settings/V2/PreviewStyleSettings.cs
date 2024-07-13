using System.Text.Json.Serialization;

namespace FancyMouse.Settings.V2;

internal class PreviewStyleSettings
{
    public PreviewStyleSettings(
        CanvasSizeSettings? canvasSize,
        CanvasStyleSettings? canvasStyle,
        ScreenStyleSettings? screenStyle)
    {
        this.CanvasSize = canvasSize;
        this.CanvasStyle = canvasStyle;
        this.ScreenStyle = screenStyle;
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
