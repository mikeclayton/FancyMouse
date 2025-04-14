using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

internal static partial class Gdi32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-gradientfill
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names and values taken from Win32Api")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Names and values taken from Win32Api")]
    [SuppressMessage("CA1712", "CA1712:DoNotPrefixEnumValuesWithTypeName", Justification = "Names match Win32 api")]
    internal enum GRADIENT_FILL : uint
    {
        GRADIENT_FILL_RECT_H = 0,
        GRADIENT_FILL_RECT_V = 1,
        GRADIENT_FILL_TRIANGLE = 2,
    }
}
