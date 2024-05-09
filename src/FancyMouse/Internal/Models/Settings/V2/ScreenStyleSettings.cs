using System.Text.Json.Serialization;

namespace FancyMouse.Internal.Models.Settings.V2;

/// <remarks>
/// Doesn't have a PaddingStyle setting like the BoxStyle class does - we don't
/// support configuring this in app settings.
/// ></remarks>
internal sealed class ScreenStyleSettings
{
    public ScreenStyleSettings(
        MarginStyleSettings? marginStyle,
        BorderStyleSettings? borderStyle,
        BackgroundStyleSettings? backgroundStyle)
    {
        this.MarginStyle = marginStyle ?? throw new ArgumentNullException(nameof(marginStyle));
        this.BorderStyle = borderStyle ?? throw new ArgumentNullException(nameof(borderStyle));
        this.BackgroundStyle = backgroundStyle ?? throw new ArgumentNullException(nameof(backgroundStyle));
    }

    [JsonPropertyName("margin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MarginStyleSettings? MarginStyle
    {
        get;
    }

    [JsonPropertyName("border")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BorderStyleSettings? BorderStyle
    {
        get;
    }

    [JsonPropertyName("background")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BackgroundStyleSettings? BackgroundStyle
    {
        get;
    }
}
