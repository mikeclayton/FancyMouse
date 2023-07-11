namespace FancyMouse.Models.Settings.V2;

public sealed class HotkeySettings
{
    public HotkeySettings(string? key, string? modifiers)
    {
        this.Key = key;
        this.Modifiers = modifiers;
    }

    public string? Key
    {
        get;
    }

    public string? Modifiers
    {
        get;
    }

    public override string ToString()
    {
        return "{" +
           $"{nameof(this.Key)}={this.Key}," +
           $"{nameof(this.Modifiers)}=\"{this.Modifiers}\"" +
           "}";
    }
}
