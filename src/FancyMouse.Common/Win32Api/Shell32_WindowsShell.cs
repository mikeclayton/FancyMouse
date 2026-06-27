using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace FancyMouse.Common.Win32Api;

internal static partial class Shell32
{
    internal static Win32Result<BOOL> Shell_NotifyIcon(NOTIFY_ICON_MESSAGE dwMessage, in NOTIFYICONDATAW lpData)
    {
        // Returns TRUE if successful, or FALSE otherwise.
        // If dwMessage is set to NIM_SETVERSION,
        // the function returns TRUE if the version was successfully changed,
        // or FALSE if the requested version is not supported.
        //
        // (note - both rules collapse into a combined SuccessIsNonZero
        // without needing to check for "dwMessage == NIM_SETVERSION")
        return PInvoke.Shell_NotifyIcon(dwMessage, lpData)
            .SuccessIsNonZero();
    }
}
