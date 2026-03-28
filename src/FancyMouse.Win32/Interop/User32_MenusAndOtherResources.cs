using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.Win32.Interop;

public static partial class User32
{
    public static BOOL AppendMenu(HMENU hMenu, MENU_ITEM_FLAGS uFlags, UINT_PTR uIDNewItem, LPCWSTR lpNewItem)
    {
        var result = NativeMethods.User32.AppendMenuW(hMenu, uFlags, uIDNewItem, lpNewItem);
        if (result == 0)
        {
            throw Win32Helper.NewWin32Exception((UINT)hMenu.Value, Marshal.GetLastPInvokeError());
        }

        return result;
    }

    public static HMENU CreatePopupMenu()
    {
        var hMenu = NativeMethods.User32.CreatePopupMenu();
        if (hMenu.IsNull)
        {
            throw Win32Helper.NewWin32Exception((UINT)hMenu.Value, Marshal.GetLastPInvokeError());
        }

        return hMenu;
    }

    public static POINT GetCursorPos()
    {
        var lpPoint = new LPPOINT(new POINT(0, 0));
        var result = NativeMethods.User32.GetCursorPos(lpPoint);
        if (!result)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }

        var point = lpPoint.ToStructure();
        lpPoint.Free();

        return point;
    }

    public static BOOL SetCursorPos(int x, int y)
    {
        var result = NativeMethods.User32.SetCursorPos(x, y);
        if (!result)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }

        return result;
    }

    public static BOOL SetMenuInfo(HMENU unnamedParam1, LPCMENUINFO unnamedParam2)
    {
        var result = NativeMethods.User32.SetMenuInfo(unnamedParam1, unnamedParam2);
        if (!result)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }

        return result;
    }

    public static BOOL TrackPopupMenuEx(HMENU hMenu, TRACK_POPUP_MENU_FLAGS uFlags, int x, int y, HWND hwnd, LPTPMPARAMS lptpm)
    {
        var result = NativeMethods.User32.TrackPopupMenuEx(hMenu, uFlags, x, y, hwnd, lptpm);
        if (!result)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }

        return result;
    }
}
