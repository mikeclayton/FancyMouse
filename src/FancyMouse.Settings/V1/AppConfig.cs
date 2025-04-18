namespace FancyMouse.Settings.V1;

/// <summary>
/// Represents the configuration file format to allow for easier
/// serialization / deserialization. This needs to be converted
/// into an AppSettings object for the main application to use.
/// </summary>
public sealed class AppConfig
{
    public AppConfig(
        FancyMouse? fancyMouse)
    {
        this.FancyMouse = fancyMouse;
    }

    public FancyMouse? FancyMouse
    {
        get;
    }
}
