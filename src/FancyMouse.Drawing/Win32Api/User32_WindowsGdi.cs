using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.Drawing.Win32Api;

internal static partial class User32
{
    internal static Win32Result<HWND> GetDesktopWindow()
    {
        // The return value is a handle to the desktop window.
        return PInvoke.GetDesktopWindow()
            .AlwaysSucceeds();
    }

    internal static Win32Result<HDC> GetWindowDC([Optional] HWND hWnd)
    {
        // If the function succeeds, the return value is a handle to a device context for the specified window.
        // If the function fails, the return value is NULL, indicating an error or an invalid hWnd parameter.
        return PInvoke.GetWindowDC(hWnd)
            .SuccessIsNonNull();
    }

    internal static Win32Result<int> ReleaseDC([Optional] HWND hWnd, HDC hDC)
    {
        // If the DC was released, the return value is 1.
        // If the DC was not released, the return value is zero.
        return PInvoke.ReleaseDC(hWnd, hDC)
            .SuccessIsNonZero();
    }
}
