using FancyMouse.Models.Keyboard;

namespace FancyMouse.HotKeys;

public sealed class HotKeyEventArgs : EventArgs
{
    internal HotKeyEventArgs(Keys key, KeyModifiers modifiers)
    {
        this.Key = key;
        this.Modifiers = modifiers;
    }

    public Keys Key
    {
        get;
    }

    public KeyModifiers Modifiers
    {
        get;
    }
}
