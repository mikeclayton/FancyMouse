using FancyMouse.WindowsHotKeys.Win32Api;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FancyMouse.WindowsHotKeys;

internal static class MessageLoop
{

    public static void Run(IntPtr hWnd)
    {

        // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessage
        while (true)
        {

            var result = Winuser.GetMessage(out var msg, hWnd, 0, 0);

            // If there is an error, the return value is -1.
            if (result == -1)
            {
                var lastWin32Error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException
                    ($"{nameof(Winuser.GetMessage)} failed with result {result}. GetLastWin32Error returned '{lastWin32Error}'.",
                    new Win32Exception(lastWin32Error)
                );
            }

            Winuser.TranslateMessage(ref msg);
            Winuser.DispatchMessage(ref msg);

            // If the function retrieves the WM_QUIT message, the return value is zero.
            if (result == 0)
            {
                break;
            }

        }

    }

}
