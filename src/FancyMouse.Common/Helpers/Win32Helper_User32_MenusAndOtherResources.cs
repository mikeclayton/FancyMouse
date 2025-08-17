using System.ComponentModel;
using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
    public static partial class User32
    {
        internal static HMENU CreatePopupMenu(HWND hWnd, int id, HOT_KEY_MODIFIERS fsModifiers, uint vk)
        {
            var hMenu = NativeMethods.User32.CreatePopupMenu();
            if (hMenu.IsNull)
            {
                throw Win32Helper.NewWin32Exception((UINT)hMenu.Value, Marshal.GetLastPInvokeError());
            }

            return hMenu;
        }

        internal static POINT GetCursorPos()
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
    }
}
