using System.Runtime.InteropServices;
using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Retrieves a handle to the desktop window. The desktop window covers the entire
    /// screen. The desktop window is the area on top of which other windows are painted.
    /// </summary>
    /// <returns>
    /// The return value is a handle to the desktop window.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdesktopwindow
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    public static partial HWND GetDesktopWindow();
}
