using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Determines whether two DPI_AWARENESS_CONTEXT values are identical.
    /// </summary>
    /// <param name="dpiContextA">The first value to compare.</param>
    /// <param name="dpiContextB">The second value to compare.</param>
    /// <returns>Returns TRUE if the values are equal, otherwise FALSE.</returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-aredpiawarenesscontextsequal
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial BOOL AreDpiAwarenessContextsEqual(
        DPI_AWARENESS_CONTEXT dpiContextA,
        DPI_AWARENESS_CONTEXT dpiContextB);
}
