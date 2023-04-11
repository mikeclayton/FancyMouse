﻿using System.ComponentModel;
using System.Runtime.InteropServices;
using FancyMouse.Drawing.Models;
using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.Helpers;

internal static class MouseHelper
{
    /// <summary>
    /// Calculates where to move the cursor to by projecting a point from
    /// the preview image onto the desktop and using that as the target location.
    /// </summary>
    /// <remarks>
    /// The preview image origin is (0, 0) but the desktop origin may be non-zero,
    /// or even negative if the primary monitor is not the at the top-left of the
    /// entire desktop rectangle, so results may contain negative coordinates.
    /// </remarks>
    public static PointInfo GetJumpLocation(PointInfo previewLocation, SizeInfo previewSize, RectangleInfo desktopBounds)
    {
        return previewLocation
            .Scale(previewSize.ScaleToFitRatio(desktopBounds.Size))
            .Offset(desktopBounds.Location);
    }

    /// <summary>
    /// Moves the cursor to the specified location.
    /// </summary>
    /// <remarks>
    /// See https://github.com/mikeclayton/FancyMouse/pull/3
    /// </remarks>
    public static void JumpCursor(PointInfo location)
    {
        // set the new cursor position *twice* - the cursor sometimes end up in
        // the wrong place if we try to cross the dead space between non-aligned
        // monitors - e.g. when trying to move the cursor from (a) to (b) we can
        // *sometimes* - for no clear reason - end up at (c) instead.
        //
        //           +----------------+
        //           |(c)    (b)      |
        //           |                |
        //           |                |
        //           |                |
        // +---------+                |
        // |  (a)    |                |
        // +---------+----------------+
        //
        // setting the position a second time seems to fix this and moves the
        // cursor to the expected location (b)
        var point = location.ToPoint();
        Cursor.Position = point;
        Cursor.Position = point;
    }

    /// <summary>
    /// Sends an input simulating an absolute mouse move to the new location.
    /// </summary>
    /// <remarks>
    /// See https://github.com/microsoft/PowerToys/issues/24523
    ///     https://github.com/microsoft/PowerToys/pull/24527
    /// </remarks>
    public static void SimulateMouseMovementEvent(Point location)
    {
        var inputs = new User32.INPUT[]
        {
            new(
                type: User32.INPUT_TYPE.INPUT_MOUSE,
                data: new User32.INPUT.DUMMYUNIONNAME(
                    mi: new User32.MOUSEINPUT(
                        dx: (int)MouseHelper.CalculateAbsoluteCoordinateX(location.X),
                        dy: (int)MouseHelper.CalculateAbsoluteCoordinateY(location.Y),
                        mouseData: 0,
                        dwFlags: User32.MOUSE_EVENT_FLAGS.MOUSEEVENTF_MOVE | User32.MOUSE_EVENT_FLAGS.MOUSEEVENTF_ABSOLUTE,
                        time: 0,
                        dwExtraInfo: ULONG_PTR.Null))),
        };
        var result = User32.SendInput(
            (uint)inputs.Length,
            new User32.LPINPUT(inputs),
            User32.INPUT.Size * inputs.Length);
        if (result != inputs.Length)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error());
        }
    }

    private static decimal CalculateAbsoluteCoordinateX(decimal x)
    {
        // If MOUSEEVENTF_ABSOLUTE value is specified, dx and dy contain normalized absolute coordinates between 0 and 65,535.
        // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
        return (x * 65535) / User32.GetSystemMetrics(User32.SYSTEM_METRICS_INDEX.SM_CXSCREEN);
    }

    internal static decimal CalculateAbsoluteCoordinateY(decimal y)
    {
        // If MOUSEEVENTF_ABSOLUTE value is specified, dx and dy contain normalized absolute coordinates between 0 and 65,535.
        // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-mouseinput
        return (y * 65535) / User32.GetSystemMetrics(User32.SYSTEM_METRICS_INDEX.SM_CYSCREEN);
    }
}
