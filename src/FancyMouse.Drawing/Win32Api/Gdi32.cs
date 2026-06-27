using System.ComponentModel;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FancyMouse.Drawing.Win32Api;

internal static class Gdi32
{
    internal static int SetStretchBltMode(HDC hdc, STRETCH_BLT_MODE mode)
    {
        var result = PInvoke.SetStretchBltMode(hdc, mode);

        // If the function succeeds, the return value is the previous stretching mode.
        // If the function fails, the return value is zero.
        // This function can return the following value: ERROR_INVALID_PARAMETER
        if ((result == 0) || (result == (int)WIN32_ERROR.ERROR_INVALID_PARAMETER))
        {
            throw new Win32Exception($"{nameof(PInvoke.SetStretchBltMode)} returned {result}");
        }

        return result;
    }

    internal static BOOL StretchBlt(HDC hdcDest, int xDest, int yDest, int wDest, int hDest, [Optional] HDC hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, ROP_CODE rop)
    {
        var result = PInvoke.StretchBlt(hdcDest, xDest, yDest, wDest, hDest, hdcSrc, xSrc, ySrc, wSrc, hSrc, rop);

        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        if (result == 0)
        {
            throw new Win32Exception($"{nameof(PInvoke.StretchBlt)} returned {result}");
        }

        return result;
    }
}
