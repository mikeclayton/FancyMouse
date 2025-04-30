using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.HotKeys;

[Flags]
public enum KeyModifiers
{
    None = 0,
    Alt = (int)HOT_KEY_MODIFIERS.MOD_ALT,
    Control = (int)HOT_KEY_MODIFIERS.MOD_CONTROL,
    Shift = (int)HOT_KEY_MODIFIERS.MOD_SHIFT,
    Windows = (int)HOT_KEY_MODIFIERS.MOD_WIN,
    NoRepeat = (int)HOT_KEY_MODIFIERS.MOD_NOREPEAT,
}
