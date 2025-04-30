using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Appends a new item to the end of the specified menu bar, drop-down menu, submenu, or shortcut menu.
    /// You can use this function to specify the content, appearance, and behavior of the menu item.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-appendmenuw
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    internal static partial BOOL AppendMenuW(
        HMENU hMenu,
        MENU_ITEM_FLAGS uFlags,
        UINT_PTR uIDNewItem,
        LPCWSTR lpNewItem
    );
}
