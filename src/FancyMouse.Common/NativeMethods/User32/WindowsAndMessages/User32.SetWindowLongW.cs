using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Changes an attribute of the specified window.
    /// The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
    /// </summary>
    /// <param name="hWnd">
    /// A handle to the window and, indirectly, the class to which the window belongs.
    /// </param>
    /// <param name="nIndex">
    /// The zero-based offset to the value to be retrieved.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is the previous value of the specified 32-bit integer.
    /// If the function fails, the return value is zero.To get extended error information, call GetLastError.
    /// If the previous value of the specified 32-bit integer is zero, and the function succeeds, the return value is zero,
    /// but the function does not clear the last error information. This makes it difficult to determine success or failure.
    /// To deal with this, you should clear the last error information by calling SetLastError with 0 before calling SetWindowLong.
    /// Then, function failure will be indicated by a return value of zero and a GetLastError result that is nonzero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongw
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial LONG SetWindowLongW(
        HWND hWnd,
        WINDOW_LONG_PTR_INDEX nIndex,
        LONG dwNewLong);
}
