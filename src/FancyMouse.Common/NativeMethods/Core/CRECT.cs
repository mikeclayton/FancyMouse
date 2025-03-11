﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// The CRECT structure defines a rectangle by the coordinates of its upper-left and lower-right corners.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/windef/ns-windef-rect
    /// </remarks>
    [SuppressMessage("Naming Rules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Name and value taken from Win32Api")]
    internal readonly struct CRECT
    {
        public static readonly CRECT Empty = new(0, 0, 0, 0);

        public readonly LONG left;
        public readonly LONG top;
        public readonly LONG right;
        public readonly LONG bottom;

        public CRECT(
            int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public CRECT(
            LONG left, LONG top, LONG right, LONG bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static int Size =>
            Marshal.SizeOf<CRECT>();

        public override string ToString()
        {
            return $"left={this.left},top={this.top},right={this.right},bottom={this.bottom}";
        }
    }
}
