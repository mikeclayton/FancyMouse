using Windows.Win32;
using Windows.Win32.Foundation;

namespace FancyMouse.WinUI3.Win32Api;

internal static partial class User32
{
    internal static Win32Result<uint> GetDpiForWindow(HWND hWnd)
    {
        // The DPI for the window, which depends on the DPI_AWARENESS of the window.
        // An invalid hwnd value will result in a return value of 0.
        return PInvoke.GetDpiForWindow(hWnd)
            .SuccessIsNonZero();
    }
}
