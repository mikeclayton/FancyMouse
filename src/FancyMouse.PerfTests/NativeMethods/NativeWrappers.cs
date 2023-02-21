using FancyMouse.PerfTests.NativeMethods.Core;

namespace FancyMouse.PerfTests.NativeMethods;

internal static class NativeWrappers
{

    public static HDC GetWindowDC(HWND hWnd)
    {
        var hdc = User32.GetWindowDC(hWnd);
        if (hdc.IsNull)
        {
            throw new InvalidOperationException($"{nameof(User32.GetWindowDC)} returned null");
        }
        return hdc;
    }

    public static HDC CreateCompatibleDC(HDC hdc)
    {
        var newHdc = Gdi32.CreateCompatibleDC(hdc);
        if (newHdc.IsNull)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.CreateCompatibleDC)} returned null");
        }
        return newHdc;
    }

    public static HBITMAP CreateCompatibleBitmap(HDC hdc, int cx, int cy)
    {
        var hBitmap = Gdi32.CreateCompatibleBitmap(hdc, cx, cy);
        if (hBitmap.IsNull)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.CreateCompatibleBitmap)} returned null");
        }
        return hBitmap;
    }

    public static HGDIOBJ SelectObject(HDC hdc, HGDIOBJ h)
    {
        var hGdiObj = Gdi32.SelectObject(hdc, h);
        if (hGdiObj.IsNull)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.SelectObject)} returned null");
        }
        return hGdiObj;
    }

    public static int SetStretchBltMode(HDC hdc, Gdi32.STRETCH_BLT_MODE mode)
    {
        var result = Gdi32.SetStretchBltMode(hdc, mode);
        if (result == 0)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.SetStretchBltMode)} returned {result}");
        }
        return result;
    }

    public static BOOL StretchBlt(
        HDC hdcDest, int xDest, int yDest, int wDest, int hDest,
        HDC hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc,
        Gdi32.ROP_CODE rop
    )
    {
        var result = Gdi32.StretchBlt(
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

    public static BOOL DeleteObject(HGDIOBJ ho)
    {
        var result = Gdi32.DeleteObject(ho);
        if (!result)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.DeleteObject)} returned {result}");
        }
        return result;
    }

    public static BOOL DeleteDC(HDC hdc)
    {
        var result = Gdi32.DeleteDC(hdc);
        if (!result)
        {
            throw new InvalidOperationException($"{nameof(Gdi32.DeleteDC)} returned {result}");
        }
        return result;
    }

    public static int ReleaseDC(HWND hWnd, HDC hDC)
    {
        var result = User32.ReleaseDC(hWnd, hDC);
        if (result == 0)
        {
            throw new InvalidOperationException($"{nameof(User32.ReleaseDC)} returned {result}");
        }
        return result;
    }

}
