using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Returns the dots per inch (dpi) value for the specified window.
    /// </summary>
    /// <param name="hwnd">The window that you want to get information about.</param>
    /// <returns>
    /// The DPI for the window, which depends on the DPI_AWARENESS of the window.
    /// See the Remarks section for more information.
    /// An invalid hwnd value will result in a return value of 0.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforwindow
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial UINT GetDpiForWindow(
        HWND hwnd);
}
