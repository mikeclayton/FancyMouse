using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Retrieves the position of the mouse cursor, in screen coordinates.
    /// </summary>
    /// <returns>
    /// Returns nonzero if successful or zero otherwise.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial BOOL GetCursorPos(
        LPPOINT lpPoint);
}
