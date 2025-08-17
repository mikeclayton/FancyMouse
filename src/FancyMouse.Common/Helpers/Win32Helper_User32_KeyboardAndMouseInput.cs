using System.ComponentModel;
using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
    public static partial class User32
    {
        public static void RegisterHotKey(HWND hWnd, int id, HOT_KEY_MODIFIERS fsModifiers, uint vk)
        {
            var result = NativeMethods.User32.RegisterHotKey(hWnd, id, fsModifiers, vk);
            if (!result)
            {
                throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
            }
        }

        public static void UnregisterHotKey(HWND hWnd, int id)
        {
            var result = NativeMethods.User32.UnregisterHotKey(hWnd, id);
            if (!result)
            {
                throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
            }
        }
    }
}
