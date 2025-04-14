using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Retrieves information about a window class, including a handle to the small icon associated with
    /// the window class. The GetClassInfo function does not retrieve a handle to the small icon.
    /// </summary>
    /// <returns>
    /// If the function finds a matching class and successfully copies the data, the return value is nonzero.
    /// If the function does not find a matching class and successfully copy the data, the return value is
    /// zero. To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getclassinfoexw
    /// </remarks>
    [DllImport(Libraries.User32, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern BOOL GetClassInfoExW(
        HINSTANCE hInstance,
        LPCWSTR lpszClass,
        [Out] LPWNDCLASSEXW lpwcx);
}
