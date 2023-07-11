namespace FancyMouse.Models.Settings.V2;

/// <summary>
/// Represents the configuration file format to allow for easier
/// serialization / deserialization. This needs to be converted
/// into an AppSettings object for the main application to use.
/// </summary>
internal sealed class AppConfig
{
    public AppConfig(
        HotkeySettings? hotkey,
        PreviewSettings? preview)
    {
        this.Hotkey = hotkey;
        this.Preview = preview;
    }

    public HotkeySettings? Hotkey
    {
        get;
    }

    public PreviewSettings? Preview
    {
        get;
    }
}
