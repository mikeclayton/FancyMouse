using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings.V2;

/// <remarks>
/// Doesn't have a MarginStyle setting like the BoxStyle class does - we don't
/// support configuring this in app settings.
/// ></remarks>
public sealed class CanvasStyleSettings
{
    public CanvasStyleSettings(
        BorderStyleSettings borderStyle,
        PaddingStyleSettings paddingStyle,
        BackgroundStyleSettings backgroundStyle)
    {
        this.BorderStyle = borderStyle ?? throw new ArgumentNullException(nameof(borderStyle));
        this.PaddingStyle = paddingStyle ?? throw new ArgumentNullException(nameof(paddingStyle));
        this.BackgroundStyle = backgroundStyle ?? throw new ArgumentNullException(nameof(backgroundStyle));
    }

    [JsonPropertyName("border")]
    public BorderStyleSettings BorderStyle
    {
        get;
    }

    [JsonPropertyName("padding")]
    public PaddingStyleSettings PaddingStyle
    {
        get;
    }

    [JsonPropertyName("background")]
    public BackgroundStyleSettings BackgroundStyle
    {
        get;
    }
}
