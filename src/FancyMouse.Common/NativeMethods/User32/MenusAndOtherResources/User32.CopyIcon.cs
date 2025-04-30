using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Copies the specified icon from another module to the current module.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is a handle to the duplicate icon.
    ///
    /// If the function fails, the return value is NULL.To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-copyicon
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial HICON CopyIcon(
        HICON hIcon);
}
