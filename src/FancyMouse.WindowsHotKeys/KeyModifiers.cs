using FancyMouse.WindowsHotKeys.Win32Api;

namespace FancyMouse.WindowsHotKeys;

[Flags]
public enum KeyModifiers
{

    None = 0,
    Alt = (int)Winuser.MOD_ALT,
    Control = (int)Winuser.MOD_CONTROL,
    Shift = (int)Winuser.MOD_SHIFT,
    Windows = (int)Winuser.MOD_WIN,
    NoRepeat = (int)Winuser.MOD_NOREPEAT

}