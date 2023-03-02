using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class Gdi32
{
    public static HBITMAP CreateCompatibleBitmap(HDC hdc, int cx, int cy)
    {
        var hBitmap = NativeMethods.Gdi32.CreateCompatibleBitmap(hdc, cx, cy);

        return hBitmap.IsNull
            ? throw new InvalidOperationException(
                $"{nameof(Gdi32.CreateCompatibleBitmap)} returned null")
            : hBitmap;
    }
}
