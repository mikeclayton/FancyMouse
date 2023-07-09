using FancyMouse.Models.Layout;
using FancyMouse.WindowsHotKeys;

namespace FancyMouse.Models.Settings;

internal sealed class AppSettings
{
    public AppSettings(
        Keystroke hotkey,
        PreviewSettings preview)
    {
        this.Hotkey = hotkey ?? throw new ArgumentNullException(nameof(hotkey));
        this.Preview = preview ?? throw new ArgumentNullException(nameof(preview));
    }

    public Keystroke Hotkey
    {
        get;
    }

    public PreviewSettings Preview
    {
        get;
    }
}
