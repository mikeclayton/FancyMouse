using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// The ScreenToClient function converts the screen coordinates of a specified
    /// point on the screen to client-area coordinates.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-screentoclient
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial BOOL ScreenToClient(
        HWND hWnd,
        LPPOINT lpPoint);
}
