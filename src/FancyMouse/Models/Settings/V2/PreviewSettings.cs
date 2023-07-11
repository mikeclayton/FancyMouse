using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings.V2;

public class PreviewSettings
{
    public PreviewSettings(
        CanvasSizeSettings canvasSize,
        CanvasStyleSettings canvasStyle,
        ScreenshotStyleSettings screenshotStyle)
    {
        this.CanvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
        this.CanvasStyle = canvasStyle ?? throw new ArgumentNullException(nameof(canvasStyle));
        this.ScreenshotStyle = screenshotStyle ?? throw new ArgumentNullException(nameof(screenshotStyle));
    }

    [JsonPropertyName("size")]
    public CanvasSizeSettings CanvasSize
    {
        get;
    }

    [JsonPropertyName("canvas")]
    public CanvasStyleSettings CanvasStyle
    {
        get;
    }

    [JsonPropertyName("screenshot")]
    public ScreenshotStyleSettings ScreenshotStyle
    {
        get;
    }
}
