using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Creates a drop-down menu, submenu, or shortcut menu. The menu is initially empty.
    /// You can insert or append menu items by using the InsertMenuItem function.
    /// You can also use the InsertMenu function to insert menu items and the AppendMenu
    /// function to append menu items.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is a handle to the newly created menu.
    /// If the function fails, the return value is NULL.To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createpopupmenu
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial HMENU CreatePopupMenu();
}
