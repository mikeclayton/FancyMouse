using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ReSharper disable CheckNamespace
namespace FancyMouse.Interop;
// ReSharper restore CheckNamespace

internal static partial class Shcore
{

    /// <summary>
    /// Sets the process-default DPI awareness level.
    /// </summary>
    /// <param name="value">The DPI awareness value to set. Possible values are from the PROCESS_DPI_AWARENESS enumeration.</param>
    /// <returns></returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellscalingapi/nf-shellscalingapi-setprocessdpiawareness
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [LibraryImport("shcore.dll")]
    public static partial int SetProcessDpiAwareness(
        PROCESS_DPI_AWARENESS value
    );

}
