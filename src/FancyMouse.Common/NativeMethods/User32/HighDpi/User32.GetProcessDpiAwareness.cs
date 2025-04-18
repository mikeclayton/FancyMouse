using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Retrieves the dots per inch (dpi) awareness of the specified process.
    /// </summary>
    /// <param name="hProcess">Handle of the process that is being queried. If this parameter is NULL, the current process is queried.</param>
    /// <param name="value">The DPI awareness of the specified process. Possible values are from the PROCESS_DPI_AWARENESS enumeration.</param>
    /// <returns>
    /// This function returns one of the following values.
    /// * S_OK - The function successfully retrieved the DPI awareness of the specified process.
    /// * E_INVALIDARG - The handle or pointer passed in is not valid.
    /// * E_ACCESSDENIED - The application does not have sufficient privileges.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellscalingapi/nf-shellscalingapi-getprocessdpiawareness
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial HRESULT GetProcessDpiAwareness(
        HANDLE hProcess,
        out PROCESS_DPI_AWARENESS value);
}
