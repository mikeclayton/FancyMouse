using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static class User32
{

    public static HWND GetDesktopWindow()
    {
        var hwnd = NativeMethods.User32.GetDesktopWindow();
        return hwnd;
    }

    public static HDC GetWindowDC(HWND hWnd)
    {
        var hdc = NativeMethods.User32.GetWindowDC(hWnd);
        if (hdc.IsNull)
        {
            throw new InvalidOperationException($"{nameof(User32.GetWindowDC)} returned null");
        }
        return hdc;
    }

    public static int ReleaseDC(HWND hWnd, HDC hDC)
    {
        var result = NativeMethods.User32.ReleaseDC(hWnd, hDC);
        if (result == 0)
        {
            throw new InvalidOperationException($"{nameof(User32.ReleaseDC)} returned {result}");
        }
        return result;
    }

}
