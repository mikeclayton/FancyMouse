﻿using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// The MonitorFromPoint function retrieves a handle to the display monitor that contains a specified point.
    /// </summary>
    /// <returns>
    /// If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor.
    /// If the point is not contained by a display monitor, the return value depends on the value of dwFlags.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-monitorfrompoint
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial HMONITOR MonitorFromPoint(
        POINT pt,
        MONITOR_FROM_FLAGS dwFlags);
}
