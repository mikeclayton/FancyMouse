using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class User32
{
    public static HDC GetWindowDC(HWND hWnd)
    {
        var hdc = NativeMethods.User32.GetWindowDC(hWnd);

        return hdc.IsNull
            ? throw new InvalidOperationException(
                $"{nameof(User32.GetWindowDC)} returned null")
            : hdc;
    }
}
