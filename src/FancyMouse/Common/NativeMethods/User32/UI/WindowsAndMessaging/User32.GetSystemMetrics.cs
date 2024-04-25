using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Retrieves the specified system metric or system configuration setting.
    ///
    /// Note that all dimensions retrieved by GetSystemMetrics are in pixels.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is the requested system metric or configuration setting.
    /// If the function fails, the return value is 0. GetLastError does not provide extended error information.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial int GetSystemMetrics(
        SYSTEM_METRICS_INDEX smIndex);
}
