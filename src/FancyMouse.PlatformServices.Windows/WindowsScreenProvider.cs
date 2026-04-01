using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.PlatformServices.Abstractions;
using FancyMouse.PlatformServices.Windows.Interop;

using static FancyMouse.PlatformServices.Windows.NativeMethods.Core;
using static FancyMouse.PlatformServices.Windows.NativeMethods.User32;

namespace FancyMouse.PlatformServices.Windows;

internal sealed class WindowsScreenProvider : IScreenProvider
{
    public IEnumerable<ScreenInfo> GetAllScreens()
    {
        // enumerate the monitors attached to the system
        var hMonitors = new List<HMONITOR>();
        var callback = new MONITORENUMPROC(
            (hMonitor, hdcMonitor, lprcMonitor, dwData) =>
            {
                hMonitors.Add(hMonitor);
                return true;
            });
        var result = User32.EnumDisplayMonitors(HDC.Null, LPCRECT.Null, callback, LPARAM.Null);

        // get detailed info about each monitor
        foreach (var hMonitor in hMonitors)
        {
            var monitorInfoPtr = new LPMONITORINFO(
                new MONITORINFO((DWORD)MONITORINFO.Size, RECT.Empty, RECT.Empty, 0));
            result = User32.GetMonitorInfoW(hMonitor, monitorInfoPtr);

            var monitorInfo = monitorInfoPtr.ToStructure();
            monitorInfoPtr.Free();

            yield return new ScreenInfo(
                handle: hMonitor,
                primary: monitorInfo.dwFlags.HasFlag(MONITOR_INFO_FLAGS.MONITORINFOF_PRIMARY),
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

    public RectangleInfo GetVirtualScreen()
    {
        return new(
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN));
    }
}
