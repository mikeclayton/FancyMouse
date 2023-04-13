using System.ComponentModel;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Screen;
using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;
using static FancyMouse.NativeMethods.User32;

namespace FancyMouse.Helpers;

internal static class ScreenHelper
{
    /// <summary>
    /// Duplicates functionality available in System.Windows.Forms.SystemInformation
    /// to reduce the dependency on WinForms
    /// </summary>
    public static RectangleInfo GetVirtualScreen()
    {
        return new(
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN),
            User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN));
    }

    public static IEnumerable<ScreenInfo> GetAllScreens()
    {
        /*
        yield return new(
            primary: false,
            displayArea: new(x: 5120, y: 358, width: 1920, height: 1080),
            workingArea: new(x: 5120, y: 358, width: 1920, height: 1008));
        yield return new(
            primary: true,
            displayArea: new(x: 0, y: 0, width: 5120, height: 1440),
            workingArea: new(x: 0, y: 0, width: 5120, height: 1392));
        */

        // enumerate the monitors attached to the system
        var hMonitors = new List<HMONITOR>();
        var result = User32.EnumDisplayMonitors(
            HDC.Null,
            LPCRECT.Null,
            (unnamedParam1, unnamedParam2, unnamedParam3, unnamedParam4) =>
            {
                hMonitors.Add(unnamedParam1);
                return true;
            },
            LPARAM.Null);
        if (!result)
        {
            throw new Win32Exception(
                $"{nameof(User32.EnumDisplayMonitors)} failed with return code {result.Value}");
        }

        // get detailed info about each monitor
        foreach (var hMonitor in hMonitors)
        {
            var monitorInfoPtr = new LPMONITORINFO(
                new MONITORINFO((uint)MONITORINFO.Size, RECT.Empty, RECT.Empty, 0));
            result = User32.GetMonitorInfoW(hMonitor, monitorInfoPtr);
            if (!result)
            {
                throw new Win32Exception(
                    $"{nameof(User32.GetMonitorInfoW)} failed with return code {result.Value}");
            }

            var monitorInfo = monitorInfoPtr.ToStructure();
            monitorInfoPtr.Free();

            yield return new ScreenInfo(
                handle: hMonitor,
                primary: monitorInfo.dwFlags.HasFlag(User32.MONITOR_INFO_FLAGS.MONITORINFOF_PRIMARY),
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

    public static HMONITOR MonitorFromPoint(
        PointInfo pt)
    {
        var hMonitor = User32.MonitorFromPoint(
            new((int)pt.X, (int)pt.Y),
            User32.MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        if (hMonitor.IsNull)
        {
            throw new InvalidOperationException($"no monitor found for point {pt}");
        }

        return hMonitor;
    }
}
