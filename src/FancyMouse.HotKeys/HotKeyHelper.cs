using System.Diagnostics.CodeAnalysis;

using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.HotKeys;

internal static class HotKeyHelper
{
    [SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
    public const MESSAGE_TYPE WM_PRIV_UNREGISTER_HOTKEY = MESSAGE_TYPE.WM_USER + 2;

    [SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
    public const MESSAGE_TYPE WM_PRIV_REGISTER_HOTKEY = MESSAGE_TYPE.WM_USER + 1;
}
