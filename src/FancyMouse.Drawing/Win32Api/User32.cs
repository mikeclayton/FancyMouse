using System.ComponentModel;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FancyMouse.Drawing.Win32Api;

internal static class User32
{
    internal static HWND GetDesktopWindow()
    {
        return PInvoke.GetDesktopWindow();
    }

    internal static HDC GetWindowDC([Optional] HWND hWnd)
    {
        var result = PInvoke.GetWindowDC(hWnd);

        // If the function succeeds, the return value is a handle to a device context for the specified window.
        // If the function fails, the return value is NULL, indicating an error or an invalid hWnd parameter.
        if (result.IsNull)
        {
            throw new Win32Exception($"{nameof(PInvoke.GetWindowDC)} returned {result}");
        }

        return result;
    }

    internal static int ReleaseDC([Optional] HWND hWnd, HDC hDC)
    {
        var result = PInvoke.ReleaseDC(hWnd, hDC);

        // If the DC was released, the return value is 1.
        // If the DC was not released, the return value is zero.
        if (result == 0)
        {
            throw new Win32Exception($"{nameof(PInvoke.ReleaseDC)} returned {result}");
        }

        return result;
    }
}
