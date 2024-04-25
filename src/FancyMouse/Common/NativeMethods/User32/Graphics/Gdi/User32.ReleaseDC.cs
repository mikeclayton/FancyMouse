using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// The ReleaseDC function releases a device context (DC), freeing it for use by other
    /// applications. The effect of the ReleaseDC function depends on the type of DC. It
    /// frees only common and window DCs. It has no effect on class or private DCs.
    /// </summary>
    /// <returns>
    /// The return value indicates whether the DC was released. If the DC was released, the return value is 1.
    /// If the DC was not released, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-releasedc
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial int ReleaseDC(
        HWND hWnd,
        HDC hDC);
}
