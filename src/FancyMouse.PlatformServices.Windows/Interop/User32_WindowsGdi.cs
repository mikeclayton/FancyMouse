using static FancyMouse.PlatformServices.Windows.NativeMethods.Core;
using static FancyMouse.PlatformServices.Windows.NativeMethods.User32;

namespace FancyMouse.PlatformServices.Windows.Interop;

internal static partial class User32
{
    public static BOOL EnumDisplayMonitors(HDC hdc, LPCRECT lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData)
    {
        var result = NativeMethods.User32.EnumDisplayMonitors(hdc, lprcClip, lpfnEnum, dwData);
        ResultHandler.ThrowIfZero(result.Value, getLastError: true);
        return result;
    }

    public static BOOL GetMonitorInfoW(HMONITOR hMonitor, LPMONITORINFO lpmi)
    {
        var result = NativeMethods.User32.GetMonitorInfoW(hMonitor, lpmi);
        ResultHandler.ThrowIfZero(result.Value, getLastError: true);
        return result;
    }
}
