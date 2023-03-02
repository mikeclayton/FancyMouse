using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class Gdi32
{
    public static int SetStretchBltMode(HDC hdc, NativeMethods.Gdi32.STRETCH_BLT_MODE mode)
    {
        var result = NativeMethods.Gdi32.SetStretchBltMode(hdc, mode);

        return result == 0
            ? throw new InvalidOperationException(
                $"{nameof(Gdi32.SetStretchBltMode)} returned {result}")
            : result;
    }
}
