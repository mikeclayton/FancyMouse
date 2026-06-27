using System.Runtime.InteropServices;

using FancyMouse.Common.Interop;
using FancyMouse.Common.Win32Api;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FancyMouse.Common.Helpers;

public static class ScreenHelper
{
    /// <summary>
    /// Duplicates functionality available in System.Windows.Forms.SystemInformation
    /// to reduce the dependency on WinForms
    /// </summary>
    private static RectangleInfo GetVirtualScreen()
    {
        return new(
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN).ThrowIfFailed().GetValue(),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN).ThrowIfFailed().GetValue(),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN).ThrowIfFailed().GetValue(),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN).ThrowIfFailed().GetValue());
    }

    public static IEnumerable<ScreenInfo> GetAllScreens()
    {
        // enumerate the monitors attached to the system
        var hMonitors = new List<HMONITOR>();
        unsafe
        {
            var callback = new MONITORENUMPROC(
                (hMonitor, hdcMonitor, lprcMonitor, dwData) =>
                {
                    hMonitors.Add(hMonitor);
                    return true;
                });

            _ = User32.EnumDisplayMonitors(HDC.Null, null, callback, (LPARAM)0)
                .ThrowIfFailed();

            // prevent callback from being collected during the enumeration
            GC.KeepAlive(callback);
        }

        // get detailed info about each monitor
        var monitorInfo = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>(),
        };
        foreach (var hMonitor in hMonitors)
        {
            _ = User32.GetMonitorInfo(hMonitor, ref monitorInfo)
                .ThrowIfFailed();
            yield return new ScreenInfo(
                handle: hMonitor,
                primary: (monitorInfo.dwFlags & PInvoke.MONITORINFOF_PRIMARY) != 0,
                displayArea: new RectangleInfo(
                    monitorInfo.rcMonitor.left,
                    monitorInfo.rcMonitor.top,
                    monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left,
                    monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top),
                workingArea: new RectangleInfo(
                    monitorInfo.rcWork.left,
                    monitorInfo.rcWork.top,
                    monitorInfo.rcWork.right - monitorInfo.rcWork.left,
                    monitorInfo.rcWork.bottom - monitorInfo.rcWork.top));
        }
    }

    public static ScreenInfo GetScreenFromPoint(
        List<ScreenInfo> screens,
        PointInfo pt)
    {
        // get the monitor handle from the point
        var hMonitor = PInvoke.MonitorFromPoint(
            new((int)pt.X, (int)pt.Y),
            MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        if (hMonitor.IsNull)
        {
            throw new InvalidOperationException($"no monitor found for point {pt}");
        }

        // find the screen with the given monitor handle
        var screen = screens
            .Single(item => item.Handle == hMonitor);
        return screen;
    }
}
