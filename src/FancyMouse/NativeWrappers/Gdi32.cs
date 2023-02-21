using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static class Gdi32
{

    public static HBITMAP CreateCompatibleBitmap(HDC hdc, int cx, int cy)
    {
        var hBitmap = NativeMethods.Gdi32.CreateCompatibleBitmap(hdc, cx, cy);
        if (hBitmap.IsNull)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.CreateCompatibleBitmap)} returned null");
        }
        return hBitmap;
    }

    public static HDC CreateCompatibleDC(HDC hdc)
    {
        var newHdc = NativeMethods.Gdi32.CreateCompatibleDC(hdc);
        if (newHdc.IsNull)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.CreateCompatibleDC)} returned null");
        }
        return newHdc;
    }

    public static BOOL DeleteDC(HDC hdc)
    {
        var result = NativeMethods.Gdi32.DeleteDC(hdc);
        if (!result)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.DeleteDC)} returned {result}");
        }
        return result;
    }

    public static BOOL DeleteObject(HGDIOBJ ho)
    {
        var result = NativeMethods.Gdi32.DeleteObject(ho);
        if (!result)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.DeleteObject)} returned {result}");
        }
        return result;
    }

    public static HGDIOBJ SelectObject(HDC hdc, HGDIOBJ h)
    {
        var hGdiObj = NativeMethods.Gdi32.SelectObject(hdc, h);
        if (hGdiObj.IsNull)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.SelectObject)} returned null");
        }
        return hGdiObj;
    }

    public static int SetStretchBltMode(HDC hdc, NativeMethods.Gdi32.STRETCH_BLT_MODE mode)
    {
        var result = NativeMethods.Gdi32.SetStretchBltMode(hdc, mode);
        if (result == 0)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.SetStretchBltMode)} returned {result}");
        }
        return result;
    }

    public static BOOL StretchBlt(
        HDC hdcDest, int xDest, int yDest, int wDest, int hDest,
        HDC hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc,
        NativeMethods.Gdi32.ROP_CODE rop
    )
    {
        var result = NativeMethods.Gdi32.StretchBlt(
            hdcDest, xDest, yDest, wDest, hDest,
            hdcSrc, xSrc, ySrc, wSrc, hSrc,
            rop
        );
        if (!result)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.StretchBlt)} returned {result}");
        }
        return result;
    }

}
