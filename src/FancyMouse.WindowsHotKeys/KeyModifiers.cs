using FancyMouse.WindowsHotKeys.Win32Api;
using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys;

[Flags]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum KeyModifiers
{

    None = 0,
    Alt = (int)Winuser.MOD_ALT,
    Control = (int)Winuser.MOD_CONTROL,
    Shift = (int)Winuser.MOD_SHIFT,
    Windows = (int)Winuser.MOD_WIN,
    NoRepeat = (int)Winuser.MOD_NOREPEAT

}