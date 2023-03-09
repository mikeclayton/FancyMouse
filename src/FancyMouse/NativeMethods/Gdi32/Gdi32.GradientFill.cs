﻿using System.Runtime.InteropServices;
using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    /// <summary>
    /// The GradientFill function fills rectangle and triangle structures.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is TRUE.
    /// If the function fails, the return value is FALSE.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-gradientfill
    /// </remarks>
    [LibraryImport(Libraries.Gdi32)]
    public static partial BOOL GradientFill(
        HDC hdc,
        TRIVERTEX[] pVertex,
        ULONG nVertex,
        PVOID pMesh,
        ULONG nMesh,
        ULONG ulMode);
}
