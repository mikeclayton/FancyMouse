using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class Kernel32
{
    /// <summary>
    /// Sets the specified event object to the signaled state.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaymonitors
    /// </remarks>
    [LibraryImport(Libraries.Kernel32, SetLastError = true)]
    internal static partial BOOL SetEvent(
        HANDLE hEvent);
}
