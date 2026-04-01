using System.Runtime.InteropServices;

using FancyMouse.PlatformServices.Abstractions;
using FancyMouse.PlatformServices.Models;
using FancyMouse.PlatformServices.Windows.Interop;

using static FancyMouse.PlatformServices.Windows.NativeMethods.Core;
using static FancyMouse.PlatformServices.Windows.NativeMethods.User32;

namespace FancyMouse.PlatformServices.Windows;

internal sealed class WindowsMouseProvider : IMouseProvider
{
    public Point GetCursorPosition()
    {
        var result = User32.GetCursorPos();
        return new(result.x, result.y);
    }

    public void SetCursorPosition(Point position)
    {
        WindowsMouseProvider.SetCursorPositioInternal(position);

        // temporary workaround for issue #1273
        WindowsMouseProvider.SimulateMouseMovementEvent(position);
    }

    private static void SetCursorPositioInternal(Point position)
    {
        // set the new cursor position *twice* - the cursor sometimes end up in
        // the wrong place if we try to cross the dead space between non-aligned
        // monitors - e.g. when trying to move the cursor from (a) to (b) through
        // the dotted area we can *sometimes* - for no clear reason - end up at
        // (c) instead.
        //
        // ..........+----------------+
        // ..........|(c)    (b)      |
        // ..........|                |
        // ..........|                |
        // ..........|                |
        // +---------+                |
        // |  (a)    |                |
        // +---------+----------------+
        //
        // setting the position a second time seems to fix this and moves the
        // cursor to the expected location (b)
        for (var i = 0; i < 2; i++)
        {
            // SetCursorPos has been known to return zero (i.e. an error),
            // *but* GetLastError result also returns zeo to indicate success
            var result = NativeMethods.User32.SetCursorPos(position.X, position.Y);
            if (result == 0)
            {
                var lastError = Marshal.GetLastPInvokeError();
                ResultHandler.HandleResult(
                    result,
                    success: lastError == 0,
                    lastError,
                    nameof(NativeMethods.User32.SetCursorPos));
            }

            var currentLocation = User32.GetCursorPos();
            if ((currentLocation.x == position.X) || (currentLocation.y == position.Y))
            {
                break;
            }
        }
    }

    /// <summary>
    /// Sends an input simulating an absolute mouse move to the new location.
    /// </summary>
    /// <remarks>
    /// See https://github.com/microsoft/PowerToys/issues/24523
    ///     https://github.com/microsoft/PowerToys/pull/24527
    /// </remarks>
    private static void SimulateMouseMovementEvent(Point location)
    {
        var inputs = new NativeMethods.User32.INPUT[]
        {
            new(
                type: INPUT_TYPE.INPUT_MOUSE,
                data: new INPUT.DUMMYUNIONNAME(
                    mi: new MOUSEINPUT(
                        dx: (int)WindowsMouseProvider.CalculateAbsoluteCoordinateX(location.X),
                        dy: (int)WindowsMouseProvider.CalculateAbsoluteCoordinateY(location.Y),
                        mouseData: 0,
                        dwFlags: MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE | MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE,
                        time: 0,
                        dwExtraInfo: ULONG_PTR.Null))),
        };
        _ = User32.SendInput((UINT)inputs.Length, new LPINPUT(inputs), INPUT.Size * inputs.Length);
    }

    private static decimal CalculateAbsoluteCoordinateX(decimal x)
    {
        // If MOUSEEVENTF_ABSOLUTE value is specified, dx and dy contain normalized absolute coordinates between 0 and 65,535.
        // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
        return (x * 65535) / User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
    }

    private static decimal CalculateAbsoluteCoordinateY(decimal y)
    {
        // If MOUSEEVENTF_ABSOLUTE value is specified, dx and dy contain normalized absolute coordinates between 0 and 65,535.
        // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
        return (y * 65535) / User32.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);
    }
}
