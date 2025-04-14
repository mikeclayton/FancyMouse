using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// The GetMonitorInfo function retrieves information about a display monitor.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaymonitors
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial BOOL GetMonitorInfoW(
        HMONITOR hMonitor,
        LPMONITORINFO lpmi);
}
