using System.Runtime.InteropServices;

using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Windows.Win32Gen;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.Windows.Helpers;

internal static class ScreenHelper
{
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
}
