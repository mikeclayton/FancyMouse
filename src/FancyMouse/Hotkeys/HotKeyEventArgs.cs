namespace FancyMouse.HotKeys;

public sealed class HotKeyEventArgs : EventArgs
{
    public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
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
