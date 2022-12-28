using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys;

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

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public Keys Key
    {
        get;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public KeyModifiers Modifiers
    {
        get;
    }

    #endregion

}
