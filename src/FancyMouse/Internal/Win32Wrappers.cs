using FancyMouse.Interop;

namespace FancyMouse.Internal;

internal static class Win32Wrappers
{

    /// <summary>
    /// Retrieves the dots per inch (dpi) awareness of the specified process.
    /// </summary>
    /// <param name="hProcess">Handle of the process that is being queried. If this parameter is NULL, the current process is queried.</param>
    /// <param name="value">The DPI awareness of the specified process. Possible values are from the PROCESS_DPI_AWARENESS enumeration.</param>
    /// <returns></returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellscalingapi/nf-shellscalingapi-getprocessdpiawareness
    /// </remarks>
    /// <returns>
    /// This function returns one of the following values.
    ///
    /// Return code    Description
    /// S_OK           The function successfully retrieved the DPI awareness of the specified process.
    /// E_INVALIDARG   The handle or pointer passed in is not valid.
    /// E_ACCESSDENIED The application does not have sufficient privileges.
    /// </returns>
    public static int GetProcessDpiAwareness(
        IntPtr hProcess,
        out Shcore.PROCESS_DPI_AWARENESS value
    )
    {
        var result = Shcore.GetProcessDpiAwareness(
            hProcess, out value
        );
        if (result != Shcore.S_OK)
        {
            throw new InvalidOperationException(
                $"{nameof(Shcore.GetProcessDpiAwareness)} returned {result}"
            );
        }
        return result;
    }

    /// <summary>
    /// Sets the process-default DPI awareness level.
    /// </summary>
    /// <param name="value">The DPI awareness value to set. Possible values are from the PROCESS_DPI_AWARENESS enumeration.</param>
    /// <returns></returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellscalingapi/nf-shellscalingapi-setprocessdpiawareness
    /// </remarks>
    public static int SetProcessDpiAwareness(
        Shcore.PROCESS_DPI_AWARENESS value
    )
    {
        var result = Shcore.SetProcessDpiAwareness(
            value
        );
        if (result != Shcore.S_OK)
        {
            throw new InvalidOperationException(
                $"{nameof(Shcore.SetProcessDpiAwareness)} returned {result}"
            );
        }
        return result;
    }

}
