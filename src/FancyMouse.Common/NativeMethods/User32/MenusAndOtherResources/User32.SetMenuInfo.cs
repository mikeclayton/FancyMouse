using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Sets information for a specified menu.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial BOOL SetMenuInfo(
        HMENU unnamedParam1,
        LPCMENUINFO unnamedParam2);
}
