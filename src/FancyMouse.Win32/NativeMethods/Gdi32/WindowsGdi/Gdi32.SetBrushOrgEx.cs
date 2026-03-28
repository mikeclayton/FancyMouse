using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class Gdi32
{
    /// <summary>
    /// The SetBrushOrgEx function sets the brush origin that GDI assigns to
    /// the next brush an application selects into the specified device context.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-setbrushorgex
    /// </remarks>
    [LibraryImport(Libraries.Gdi32)]
    internal static partial BOOL SetBrushOrgEx(
        HDC hdc,
        int x,
        int y,
        LPPOINT lppt);
}
