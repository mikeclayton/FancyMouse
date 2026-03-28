using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.Shell32;

namespace FancyMouse.Win32.Interop;

public static partial class Shell32
{
    public static BOOL Shell_NotifyIcon(NOTIFY_ICON_MESSAGE dwMessage, PNOTIFYICONDATAW lpData)
    {
        var result = NativeMethods.Shell32.Shell_NotifyIconW(dwMessage, lpData);
        if (!result)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value);
        }

        return result;
    }
}
