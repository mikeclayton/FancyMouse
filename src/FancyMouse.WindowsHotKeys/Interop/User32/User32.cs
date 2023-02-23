using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.WindowsHotKeys.Interop;

[SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
internal static partial class User32
{
    public const int HWND_MESSAGE = -3;

    public const uint CW_USEDEFAULT = 0x80000000;
}
