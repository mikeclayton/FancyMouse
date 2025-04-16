using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class Kernel32
{
    /// <summary>
    /// Sets the last-error code for the calling thread.
    /// </summary>
    /// <param name="dwErrCode">The last-error code for the thread.</param>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-setlasterror
    /// </remarks>
    [LibraryImport(Libraries.Kernel32)]
    internal static partial void SetLastError(
        DWORD dwErrCode);
}
