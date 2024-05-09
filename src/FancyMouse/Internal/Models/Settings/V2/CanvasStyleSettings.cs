using System.Text.Json.Serialization;

namespace FancyMouse.Internal.Models.Settings.V2;

/// <remarks>
/// Doesn't have a MarginStyle setting like the BoxStyle class does - we don't
/// support configuring this in app settings.
/// </remarks>
internal sealed class CanvasStyleSettings
{
    public CanvasStyleSettings(
        BorderStyleSettings? borderStyle,
        PaddingStyleSettings? paddingStyle,
        BackgroundStyleSettings? backgroundStyle)
    {
        this.BorderStyle = borderStyle ?? throw new ArgumentNullException(nameof(borderStyle));
        this.PaddingStyle = paddingStyle ?? throw new ArgumentNullException(nameof(paddingStyle));
        this.BackgroundStyle = backgroundStyle ?? throw new ArgumentNullException(nameof(backgroundStyle));
    }

    [JsonPropertyName("border")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BorderStyleSettings? BorderStyle
    {
        get;
    }

    [JsonPropertyName("padding")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaddingStyleSettings? PaddingStyle
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
