using System.Runtime.InteropServices;
using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Determines whether the specified window handle identifies an existing window.
    /// </summary>
    /// <returns>
    /// If the window handle identifies an existing window, the return value is nonzero.
    /// If the window handle does not identify an existing window, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-iswindow
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial BOOL IsWindow(
        HWND hWnd);
}
