using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Retrieves information about the specified window.
    /// The function also retrieves the value at a specified offset into the extra window memory.
    /// </summary>
    /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
    /// <param name="nIndex">
    /// The zero-based offset to the value to be retrieved.
    /// Valid values are in the range zero through the number of bytes of extra window memory,
    /// minus the size of a LONG_PTR.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is the requested value.
    /// If the function fails, the return value is zero.To get extended error information, call GetLastError.
    /// If SetWindowLong or SetWindowLongPtr has not been called previously, GetWindowLongPtr returns zero
    /// for values in the extra window or class memory.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongw
    /// </remarks>
    [LibraryImport(Libraries.User32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial LONG_PTR GetWindowLongPtrW(
        HWND hWnd,
        WINDOW_LONG_PTR_INDEX nIndex);
}
