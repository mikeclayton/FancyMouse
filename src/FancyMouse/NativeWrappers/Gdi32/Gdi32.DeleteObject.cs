using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class Gdi32
{
    public static BOOL DeleteObject(HGDIOBJ ho)
    {
        var result = NativeMethods.Gdi32.DeleteObject(ho);

        if (!result)
        {
            throw new InvalidOperationException(
                $"{nameof(Gdi32.DeleteObject)} returned {result}");
        }

        return result;
    }
}
