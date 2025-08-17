using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Retrieves information about the specified window.
    /// The function also retrieves the 32-bit (DWORD) value at the specified offset into the extra window memory.
    /// </summary>
    /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
    /// <param name="nIndex">
    /// The zero-based offset to the value to be retrieved.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is the requested value.
    /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// If SetWindowLong has not been called previously, GetWindowLong returns zero for values in the extra window or class memory.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongw
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial LONG GetWindowLongW(
        HWND hWnd,
        WINDOW_LONG_PTR_INDEX nIndex);
}
