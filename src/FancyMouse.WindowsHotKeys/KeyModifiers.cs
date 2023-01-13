using FancyMouse.WindowsHotKeys.Interop;
using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys;

[Flags]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum KeyModifiers
{

    None = 0,
    Alt = (int)User32.RegisterHotKeyModifiers.MOD_ALT,
    Control = (int)User32.RegisterHotKeyModifiers.MOD_CONTROL,
    Shift = (int)User32.RegisterHotKeyModifiers.MOD_SHIFT,
    Windows = (int)User32.RegisterHotKeyModifiers.MOD_WIN,
    NoRepeat = (int)User32.RegisterHotKeyModifiers.MOD_NOREPEAT

}