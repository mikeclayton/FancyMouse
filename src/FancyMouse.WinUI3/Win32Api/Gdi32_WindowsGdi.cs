using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.WinUI3.Win32Api;

internal static partial class Gdi32
{
    internal static Win32Result<HRGN> CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy)
    {
        // If the function succeeds, the return value identifies a region.
        // If the function fails, the return value is NULL.
        return PInvoke.CreateRoundRectRgn(x1, y1, x2, y2, cx, cy)
            .SuccessIsNonNull();
    }

    internal static Win32Result<BOOL> DeleteObject(HGDIOBJ ho)
    {
        // If the function succeeds, the return value is nonzero.
        // If the specified handle is not valid or is currently selected into a DC, the return value is zero.
        return PInvoke.DeleteObject(ho)
            .SuccessIsNonZero();
    }

    internal static Win32Result<int> SetWindowRgn(HWND hWnd, HRGN hRgn, BOOL bRedraw)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        return PInvoke.SetWindowRgn(hWnd, hRgn, bRedraw)
            .SuccessIsNonZero();
    }
}
