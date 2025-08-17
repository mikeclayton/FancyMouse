﻿using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

[SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
public static partial class User32
{
    /// <summary>
    /// A set of flags that represent attributes of the display monitor.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-monitorinfo
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names and values taken from Win32Api")]
    internal enum MONITOR_INFO_FLAGS : uint
    {
        MONITORINFOF_PRIMARY = 1,
    }
}
