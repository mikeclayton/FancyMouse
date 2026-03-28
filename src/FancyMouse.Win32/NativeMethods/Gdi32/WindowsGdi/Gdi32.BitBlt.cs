using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class Gdi32
{
    /// <summary>
    /// The BitBlt function performs a bit-block transfer of the color data corresponding to a
    /// rectangle of pixels from the specified source device context into a destination device context.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-bitblt
    /// </remarks>
    [LibraryImport(Libraries.Gdi32, SetLastError = true)]
    internal static partial BOOL BitBlt(
        HDC hdc,
        int x,
        int y,
        int cx,
        int cy,
        HDC hdcSrc,
        int x1,
        int y1,
        ROP_CODE rop);
}
