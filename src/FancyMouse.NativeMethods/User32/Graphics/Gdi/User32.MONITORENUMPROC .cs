﻿using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// A MonitorEnumProc function is an application-defined callback function that is called by the EnumDisplayMonitors function.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-monitorenumproc
    /// </remarks>
    internal delegate BOOL MONITORENUMPROC(
        HMONITOR unnamedParam1,
        HDC unnamedParam2,
        LPRECT unnamedParam3,
        LPARAM unnamedParam4);
}
