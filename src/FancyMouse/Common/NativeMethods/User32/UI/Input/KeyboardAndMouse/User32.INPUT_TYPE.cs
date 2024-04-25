using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

[SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
internal static partial class User32
{
    /// <summary>
    /// The type of the input event.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-input
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names and values taken from Win32Api")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Names and values taken from Win32Api")]
    internal enum INPUT_TYPE : uint
    {
        INPUT_MOUSE = 0,
        INPUT_KEYBOARD = 1,
        INPUT_HARDWARE = 2,
    }
}
