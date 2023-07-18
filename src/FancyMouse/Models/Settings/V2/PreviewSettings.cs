using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings.V2;

public class PreviewSettings
{
    public PreviewSettings(
        CanvasSizeSettings? canvasSize,
        CanvasStyleSettings? canvasStyle,
        ScreenshotStyleSettings? screenshotStyle)
    {
        this.CanvasSize = canvasSize;
        this.CanvasStyle = canvasStyle;
        this.ScreenshotStyle = screenshotStyle;
    }

    [JsonPropertyName("size")]
    public CanvasSizeSettings? CanvasSize
    {
        get;
    }

    [JsonPropertyName("canvas")]
    public CanvasStyleSettings? CanvasStyle
    {
        get;
    }

    [JsonPropertyName("screenshot")]
    public ScreenshotStyleSettings? ScreenshotStyle
    {
        get;
    }
}
