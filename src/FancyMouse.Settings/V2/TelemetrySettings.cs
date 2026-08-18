using System.Text.Json.Serialization;

namespace FancyMouse.Settings.V2;

public sealed class TelemetrySettings
{
    public TelemetrySettings(bool? enabled)
    {
        this.Enabled = enabled;
    }

    [JsonPropertyName("enabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Enabled
    {
        get;
    }
}
