using System.Runtime.InteropServices;
using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.Gdi32;

namespace FancyMouse.Win32.Interop;

public static partial class Gdi32
{
    public static int SetStretchBltMode(HDC hdc, STRETCH_BLT_MODE mode)
    {
        var result = NativeMethods.Gdi32.SetStretchBltMode(hdc, mode);
        if (result == 0)
        {
            throw Win32Helper.NewWin32Exception((UINT)result, Marshal.GetLastPInvokeError());
        }

        return result;
    }

    public static BOOL StretchBlt(HDC hdcDest, int xDest, int yDest, int wDest, int hDest, HDC hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, ROP_CODE rop)
    {
        var result = NativeMethods.Gdi32.StretchBlt(hdcDest, xDest, yDest, wDest, hDest, hdcSrc, xSrc, ySrc, wSrc, hSrc, rop);
        if (result == 0)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }

        return result;
    }
}
