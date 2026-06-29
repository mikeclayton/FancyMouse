using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace FancyMouse.HotKeys.Win32Api;

internal static partial class User32
{
    internal static Win32Result<BOOL> RegisterHotKey(HWND hWnd, int id, HOT_KEY_MODIFIERS fsModifiers, uint vk)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        // To get extended error information, call GetLastError.
        return PInvoke.RegisterHotKey(hWnd, id, fsModifiers, vk)
            .SuccessIsNonZero()
            .UsesLastError();
    }

    internal static Win32Result<BOOL> UnregisterHotKey(HWND hWnd, int id)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        // To get extended error information, call GetLastError.
        return PInvoke.UnregisterHotKey(hWnd, id)
            .SuccessIsNonZero()
            .UsesLastError();
    }
}
