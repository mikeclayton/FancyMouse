using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.Win32.Interop;

public static partial class User32
{
    public static UINT GetDpiForWindow(HWND hwnd)
    {
        return NativeMethods.User32.GetDpiForWindow(hwnd);
    }

    public static DPI_AWARENESS GetAwarenessFromDpiAwarenessContext(DPI_AWARENESS_CONTEXT value)
    {
        return NativeMethods.User32.GetAwarenessFromDpiAwarenessContext(value);
    }

    public static DPI_AWARENESS_CONTEXT GetThreadDpiAwarenessContext()
    {
        return NativeMethods.User32.GetThreadDpiAwarenessContext();
    }
}
