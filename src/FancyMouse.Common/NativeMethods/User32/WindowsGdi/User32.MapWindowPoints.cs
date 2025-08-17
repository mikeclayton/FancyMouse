using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// The MapWindowPoints function converts (maps) a set of points from a
    /// coordinate space relative to one window to a coordinate space relative to another window.
    /// </summary>
    /// <param name="hWndFrom">
    /// A handle to the window from which points are converted.
    /// If this parameter is NULL or HWND_DESKTOP, the points are presumed to be in screen coordinates.
    /// </param>
    /// <param name="hWndTo">
    /// A handle to the window to which points are converted.
    /// If this parameter is NULL or HWND_DESKTOP, the points are converted to screen coordinates.
    /// </param>
    /// <param name="lpPoints">
    /// A pointer to an array of POINT structures that contain the set of points to be converted.
    /// The points are in device units. This parameter can also point to a RECT structure,
    /// in which case the cPoints parameter should be set to 2.
    /// </param>
    /// <param name="cPoints">
    /// The number of POINT structures in the array pointed to by the lpPoints parameter.
    /// </param>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mapwindowpoints
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial int MapWindowPoints(
        HWND hWndFrom,
        HWND hWndTo,
        LPPOINT lpPoints,
        UINT cPoints);
}
