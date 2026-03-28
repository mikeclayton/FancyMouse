using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.Win32.Interop;

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

    public static UINT SendInput(UINT cInputs, LPINPUT pInputs, int cbSize)
    {
        var result = NativeMethods.User32.SendInput(cInputs, pInputs, cbSize);
        if (result != cInputs)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }

        return result;
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
