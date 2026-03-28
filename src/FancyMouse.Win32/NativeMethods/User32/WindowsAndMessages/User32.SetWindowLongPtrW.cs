using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Changes an attribute of the specified window.
    /// The function also sets a value at the specified offset in the extra window memory.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is the previous value of the specified offset
    /// If the function fails, the return value is zero.To get extended error information, call GetLastError.
    /// If the previous value is zero and the function succeeds, the return value is zero,
    /// but the function does not clear the last error information. To determine success or failure,
    /// clear the last error information by calling SetLastError with 0, then call SetWindowLongPtr.
    /// Function failure will be indicated by a return value of zero and a GetLastError result that is nonzero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongptrw
    /// </remarks>
    [LibraryImport(Libraries.User32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    internal static partial LONG_PTR SetWindowLongPtrW(
        HWND hWnd,
        WINDOW_LONG_PTR_INDEX nIndex,
        LONG_PTR dwNewLong);
}
