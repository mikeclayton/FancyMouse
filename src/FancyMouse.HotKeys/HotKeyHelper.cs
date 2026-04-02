using System.Diagnostics.CodeAnalysis;
using Windows.Win32;

namespace FancyMouse.HotKeys;

internal static class HotKeyHelper
{
    [SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
    public const uint WM_PRIV_UNREGISTER_HOTKEY = PInvoke.WM_USER + 2;

    [SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
    public const uint WM_PRIV_REGISTER_HOTKEY = PInvoke.WM_USER + 1;
}
