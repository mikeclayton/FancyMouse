using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class Gdi32
{
    public static HDC CreateCompatibleDC(HDC hdc)
    {
        var newHdc = NativeMethods.Gdi32.CreateCompatibleDC(hdc);

        if (newHdc.IsNull)
        {
            throw new InvalidOperationException(
                $"{nameof(Gdi32.CreateCompatibleDC)} returned null");
        }

        return newHdc;
    }
}
