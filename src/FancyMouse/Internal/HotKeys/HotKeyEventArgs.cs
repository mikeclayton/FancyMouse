namespace FancyMouse.Internal.HotKeys;

internal sealed class HotKeyEventArgs : EventArgs
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
