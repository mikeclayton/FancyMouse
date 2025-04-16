using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

[SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
public static partial class User32
{
    internal const uint CW_USEDEFAULT = 0x80000000;

    /// <summary>
    /// See https://learn.microsoft.com/en-us/windows/win32/hidpi/wm-dpichanged
    /// </summary>
    public const int USER_DEFAULT_SCREEN_DPI = 96;
}
