namespace FancyMouse.Models.Settings.V2;

/// <summary>
/// Represents the configuration file format to allow for easier
/// serialization / deserialization. This needs to be converted
/// into an AppSettings object for the main application to use.
/// </summary>
internal sealed class AppConfig
{
    public AppConfig(
        string? hotkey,
        PreviewSettings? preview)
    {
        this.Hotkey = hotkey;
        this.Preview = preview;
    }

    public string? Hotkey
    {
        get;
    }

    public PreviewSettings? Preview
    {
        get;
    }
}
