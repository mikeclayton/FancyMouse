﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Used by SendInput to store information for synthesizing input events such as keystrokes, mouse movement, and mouse clicks.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-input
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Parameter name matches Win32 api")]
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct INPUT
    {
        public readonly INPUT_TYPE type;
        public readonly DUMMYUNIONNAME data;

        public INPUT(INPUT_TYPE type, DUMMYUNIONNAME data)
        {
            this.type = type;
            this.data = data;
        }

        public static int Size =>
            Marshal.SizeOf<INPUT>();

        [StructLayout(LayoutKind.Explicit)]
        public readonly struct DUMMYUNIONNAME
        {
            [FieldOffset(0)]
            public readonly MOUSEINPUT mi;
            [FieldOffset(0)]
            public readonly KEYBDINPUT ki;
            [FieldOffset(0)]
            public readonly HARDWAREINPUT hi;

            public DUMMYUNIONNAME(MOUSEINPUT mi)
            {
                this.mi = mi;
            }
        }
    }
}
