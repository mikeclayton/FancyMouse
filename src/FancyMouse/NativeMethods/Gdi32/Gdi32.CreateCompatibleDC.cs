﻿using System.Runtime.InteropServices;
using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    /// <summary>
    /// The CreateCompatibleDC function creates a memory device context (DC) compatible with
    /// the specified device.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is the handle to a memory DC.
    /// If the function fails, the return value is NULL.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createcompatibledc
    /// </remarks>
    [LibraryImport(Libraries.Gdi32)]
    public static partial HDC CreateCompatibleDC(
        HDC hdc);
}
