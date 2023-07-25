using System.Runtime.InteropServices;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Destroys the specified window. The function sends WM_DESTROY and WM_NCDESTROY messages to the window
    /// to deactivate it and remove the keyboard focus from it. The function also destroys the window's menu,
    /// flushes the thread message queue, destroys timers, removes clipboard ownership, and breaks the clipboard
    /// viewer chain (if the window is at the top of the viewer chain).
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroywindow
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial BOOL DestroyWindow(
        HWND hWnd);
}
