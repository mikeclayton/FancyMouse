using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class User32
{
    public static int ReleaseDC(HWND hWnd, HDC hDC)
    {
        var result = NativeMethods.User32.ReleaseDC(hWnd, hDC);

        if (result == 0)
        {
            throw new InvalidOperationException(
                $"{nameof(User32.ReleaseDC)} returned {result}");
        }

        return result;
    }
}
