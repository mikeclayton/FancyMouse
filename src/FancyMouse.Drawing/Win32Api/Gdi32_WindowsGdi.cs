using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.Drawing.Win32Api;

internal static partial class Gdi32
{
    internal static Win32Result<int> SetStretchBltMode(HDC hdc, STRETCH_BLT_MODE mode)
    {
        var returnValue = PInvoke.SetStretchBltMode(hdc, mode);

        // If the function succeeds, the return value is the previous stretching mode.
        // If the function fails, the return value is zero.
        // This function can return the following value: ERROR_INVALID_PARAMETER
        var result = returnValue.SuccessIsNonZero();
        if (returnValue == (int)WIN32_ERROR.ERROR_INVALID_PARAMETER)
        {
            return result.Failed();
        }

        return result;
    }

    internal static Win32Result<BOOL> StretchBlt(HDC hdcDest, int xDest, int yDest, int wDest, int hDest, [Optional] HDC hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, ROP_CODE rop)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        return PInvoke.StretchBlt(hdcDest, xDest, yDest, wDest, hDest, hdcSrc, xSrc, ySrc, wSrc, hSrc, rop)
            .SuccessIsNonZero();
    }
}
