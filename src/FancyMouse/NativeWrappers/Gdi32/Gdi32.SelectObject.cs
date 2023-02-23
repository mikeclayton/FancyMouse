using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class Gdi32
{
    public static HGDIOBJ SelectObject(HDC hdc, HGDIOBJ h)
    {
        var hGdiObj = NativeMethods.Gdi32.SelectObject(hdc, h);

        if (hGdiObj.IsNull)
        {
            throw new InvalidOperationException(
                $"{nameof(Gdi32.SelectObject)} returned null");
        }

        if ((HANDLE)hGdiObj == NativeMethods.Gdi32.HGDI_ERROR)
        {
            throw new InvalidOperationException(
                $"{nameof(Gdi32.SelectObject)} returned {nameof(NativeMethods.Gdi32.HGDI_ERROR)} ({NativeMethods.Gdi32.HGDI_ERROR})");
        }

        return hGdiObj;
    }
}
