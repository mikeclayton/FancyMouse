using System.ComponentModel;
using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.Win32.Interop;

public static partial class User32
{
    public static BOOL EnumDisplayMonitors(HDC hdc, LPCRECT lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData)
    {
        var result = NativeMethods.User32.EnumDisplayMonitors(hdc, lprcClip, lpfnEnum, dwData);
        if (result.Value == 0)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw new Win32Exception(lastError);
        }

        return result;
    }

    public static HDC GetWindowDC(HWND hWnd)
    {
        return NativeMethods.User32.GetWindowDC(hWnd);
    }

    public static BOOL GetMonitorInfo(HMONITOR hMonitor, LPMONITORINFO lpmi)
    {
        var result = NativeMethods.User32.GetMonitorInfoW(hMonitor, lpmi);
        if (result.Value == 0)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw new Win32Exception(lastError);
        }

        return result;
    }

    /// <summary>
    /// The MapWindowPoints function converts (maps) a set of points from a
    /// coordinate space relative to one window to a coordinate space relative to another window.
    /// </summary>
    public static POINT MapWindowPoint(HWND hWndFrom, HWND hWndTo, POINT point)
    {
        var cPoints = (UINT)1;
        var lpPoint = new LPPOINT(point);
        var result = NativeMethods.User32.MapWindowPoints(hWndFrom, hWndTo, lpPoint, cPoints);
        if (result == 0)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw new Win32Exception(lastError);
        }

        var mappedPoint = lpPoint.ToStructure();
        lpPoint.Free();

        return mappedPoint;
    }

    public static HMONITOR MonitorFromPoint(POINT pt, MONITOR_FROM_FLAGS dwFlags)
    {
        return NativeMethods.User32.MonitorFromPoint(pt, dwFlags);
    }

    public static POINT ScreenToClient(HWND hWnd, POINT point)
    {
        var lpPoint = new LPPOINT(point);
        var result = NativeMethods.User32.ScreenToClient(hWnd, lpPoint);
        if (result == 0)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw new Win32Exception(lastError);
        }

        var mappedPoint = lpPoint.ToStructure();
        lpPoint.Free();

        return mappedPoint;
    }

    public static int ReleaseDC(HWND hWnd, HDC hDC)
    {
        return NativeMethods.User32.ReleaseDC(hWnd, hDC);
    }
}
