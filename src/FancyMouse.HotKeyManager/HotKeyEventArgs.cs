namespace FancyMouse.HotKeyManager;

public sealed class HotKeyEventArgs : EventArgs
{

    #region Constructors

    public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
    {
        this.Key = key;
        this.Modifiers = modifiers;
    }

    #endregion

    #region Properties

    public Keys Key
    {
        get;
    }

    public KeyModifiers Modifiers
    {
        get;
    }

    #endregion

}
