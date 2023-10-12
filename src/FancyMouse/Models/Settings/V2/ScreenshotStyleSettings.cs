using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings.V2;

/// <remarks>
/// Doesn't have a PaddingStyle setting like the BoxStyle class does - we don't
/// support configuring this in app settings.
/// ></remarks>
public sealed class ScreenshotStyleSettings
{
    public ScreenshotStyleSettings(
        MarginStyleSettings? marginStyle,
        BorderStyleSettings? borderStyle,
        BackgroundStyleSettings? backgroundStyle)
    {
        this.MarginStyle = marginStyle ?? throw new ArgumentNullException(nameof(marginStyle));
        this.BorderStyle = borderStyle ?? throw new ArgumentNullException(nameof(borderStyle));
        this.BackgroundStyle = backgroundStyle ?? throw new ArgumentNullException(nameof(backgroundStyle));
    }

    [JsonPropertyName("margin")]
    public MarginStyleSettings? MarginStyle
    {
        get;
    }

    [JsonPropertyName("border")]
    public BorderStyleSettings? BorderStyle
    {
        get;
    }

    [JsonPropertyName("background")]
    public BackgroundStyleSettings? BackgroundStyle
    {
        get;
    }
}
