﻿using System.Diagnostics.CodeAnalysis;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    /// <summary>
    /// The TRIVERTEX structure contains color information and position information.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-trivertex
    /// </remarks>
    [SuppressMessage("Naming Rules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Name and value taken from Win32Api")]
    internal readonly struct TRIVERTEX
    {
        public readonly LONG x;
        public readonly LONG y;
        public readonly COLOR16 Red;
        public readonly COLOR16 Green;
        public readonly COLOR16 Blue;
        public readonly COLOR16 Alpha;
    }
}
