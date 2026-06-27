using System.Drawing;
using System.Runtime.InteropServices;

using FancyMouse.Common.Win32Api;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.Common.Win32Api;

internal static partial class User32
{
    internal static Win32Result<BOOL> EnumDisplayMonitors([Optional] HDC hdc, [Optional] RECT? lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        return PInvoke.EnumDisplayMonitors(hdc, lprcClip, lpfnEnum, dwData)
            .SuccessIsNonZero();
    }

    internal static Win32Result<BOOL> GetMonitorInfo(HMONITOR hMonitor, ref MONITORINFO lpmi)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        return PInvoke.GetMonitorInfo(hMonitor, ref lpmi)
            .SuccessIsNonZero();
    }

    internal static Win32Result<HMONITOR> MonitorFromPoint(Point pt, MONITOR_FROM_FLAGS dwFlags)
    {
        // If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor.
        // If the point is not contained by a display monitor, the return value depends on the value of dwFlags.
        return PInvoke.MonitorFromPoint(pt, dwFlags)
            .AlwaysSucceeds();
    }

    internal static unsafe Win32Result<BOOL> ScreenToClient(HWND hWnd, ref Point lpPoint)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        return PInvoke.ScreenToClient(hWnd, ref lpPoint)
            .SuccessIsNonZero();
    }
}
