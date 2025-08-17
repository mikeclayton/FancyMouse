using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class Gdi32
{
    /// <summary>
    /// The SetStretchBltMode function sets the bitmap stretching mode in the specified device context.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is the previous stretching mode.
    /// If the function fails, the return value is zero.
    /// This function can return the following value.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-setstretchbltmode
    /// </remarks>
    [LibraryImport(Libraries.Gdi32)]
    internal static partial int SetStretchBltMode(
        HDC hdc,
        STRETCH_BLT_MODE mode);
}
