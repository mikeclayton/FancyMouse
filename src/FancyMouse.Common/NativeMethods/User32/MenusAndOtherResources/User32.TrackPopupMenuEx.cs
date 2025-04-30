using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Displays a shortcut menu at the specified location and tracks the selection of items
    /// on the shortcut menu. The shortcut menu can appear anywhere on the screen.
    /// </summary>
    /// <returns>
    /// If you specify TPM_RETURNCMD in the fuFlags parameter, the return value is the menu-item
    /// identifier of the item that the user selected. If the user cancels the menu without making
    /// a selection, or if an error occurs, the return value is zero.
    ///
    /// If you do not specify TPM_RETURNCMD in the fuFlags parameter, the return value is nonzero
    /// if the function succeeds and zero if it fails.To get extended error information, call
    /// GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-trackpopupmenuex
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial BOOL TrackPopupMenuEx(
        HMENU hMenu,
        TRACK_POPUP_MENU_FLAGS uFlags,
        int x,
        int y,
        HWND hwnd,
        LPTPMPARAMS lptpm);
}
