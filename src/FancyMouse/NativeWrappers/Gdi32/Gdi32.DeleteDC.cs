using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class Gdi32
{
    public static BOOL DeleteDC(HDC hdc)
    {
        var result = NativeMethods.Gdi32.DeleteDC(hdc);

        return result
            ? result
            : throw new InvalidOperationException(
                $"{nameof(Gdi32.DeleteDC)} returned {result}");
    }
}
