using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeWrappers;

internal static partial class User32
{
    public static HWND GetDesktopWindow()
    {
        var hwnd = NativeMethods.User32.GetDesktopWindow();
        return hwnd;
    }
}
