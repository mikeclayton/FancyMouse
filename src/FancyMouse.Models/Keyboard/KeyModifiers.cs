namespace FancyMouse.Models.Keyboard;

/// <remarks>
/// The values must match Win32's <c>HOT_KEY_MODIFIERS</c> (<c>MOD_ALT</c> etc), as
/// callers may use them in Win32 api calls like RegisterHotKey.
/// </remarks>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Alt = 0x0001, // HOT_KEY_MODIFIERS.MOD_ALT
    Control = 0x0002, // HOT_KEY_MODIFIERS.MOD_CONTROL
    Shift = 0x0004, // HOT_KEY_MODIFIERS.MOD_SHIFT
    Windows = 0x0008, // HOT_KEY_MODIFIERS.MOD_WIN
}
