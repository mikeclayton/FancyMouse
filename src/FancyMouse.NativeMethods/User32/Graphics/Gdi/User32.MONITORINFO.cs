﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Used by SendInput to store information for synthesizing input events such as keystrokes, mouse movement, and mouse clicks.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-monitorinfo
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Parameter name matches Win32 api")]
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct MONITORINFO
    {
        public readonly DWORD cbSize;
        public readonly RECT rcMonitor;
        public readonly RECT rcWork;
        public readonly MONITOR_INFO_FLAGS dwFlags;

        public MONITORINFO(DWORD cbSize, RECT rcMonitor, RECT rcWork, MONITOR_INFO_FLAGS dwFlags)
        {
            this.cbSize = cbSize;
            this.rcMonitor = rcMonitor;
            this.rcWork = rcWork;
            this.dwFlags = dwFlags;
        }

        public static int Size =>
            Marshal.SizeOf(typeof(INPUT));
    }
}
