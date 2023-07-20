using System.Runtime.InteropServices;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Shell32
{
    /// <summary>
    /// Sends a message to the taskbar's status area.
    /// </summary>
    /// <returns>
    /// Returns TRUE if successful, or FALSE otherwise.
    /// If dwMessage is set to NIM_SETVERSION, the function returns TRUE if the version
    /// was successfully changed, or FALSE if the requested version is not supported.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw
    /// </remarks>
    [LibraryImport(Libraries.Shell32)]
    internal static partial BOOL Shell_NotifyIconW(
        NOTIFY_ICON_MESSAGE dwMessage,
        PNOTIFYICONDATAW lpData);
}
