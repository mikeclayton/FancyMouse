using FancyMouse.NativeMethods;

namespace FancyMouse.WindowsHotKeys;

[Flags]
public enum KeyModifiers
{
    None = 0,
    Alt = (int)User32.HOT_KEY_MODIFIERS.MOD_ALT,
    Control = (int)User32.HOT_KEY_MODIFIERS.MOD_CONTROL,
    Shift = (int)User32.HOT_KEY_MODIFIERS.MOD_SHIFT,
    Windows = (int)User32.HOT_KEY_MODIFIERS.MOD_WIN,
    NoRepeat = (int)User32.HOT_KEY_MODIFIERS.MOD_NOREPEAT,
}
