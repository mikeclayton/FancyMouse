using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// A MonitorEnumProc function is an application-defined callback function that is called by the EnumDisplayMonitors function.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-monitorenumproc
    /// </remarks>
    public delegate BOOL MONITORENUMPROC(
        HMONITOR unnamedParam1,
        HDC unnamedParam2,
        LPRECT unnamedParam3,
        LPARAM unnamedParam4);
}
