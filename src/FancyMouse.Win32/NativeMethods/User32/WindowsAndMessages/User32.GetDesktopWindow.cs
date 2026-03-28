using System.Runtime.InteropServices;
using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
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
    internal static partial HWND GetDesktopWindow();
}
