using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class Gdi32
{
    /// <summary>
    /// The SelectObject function selects an object into the specified device context (DC). The new object replaces the previous object of the same type.
    /// </summary>
    /// <returns>
    /// If the selected object is not a region and the function succeeds, the return value is a handle to the object being replaced.
    /// If the selected object is a region and the function succeeds, the return value is one of the following values.
    ///
    /// Value Meaning
    /// SIMPLEREGION  Region consists of a single rectangle.
    /// COMPLEXREGION Region consists of more than one rectangle.
    /// NULLREGION    Region is empty.
    ///
    /// If an error occurs and the selected object is not a region, the return value is NULL. Otherwise, it is HGDI_ERROR.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-selectobject
    /// </remarks>
    [LibraryImport(Libraries.Gdi32)]
    internal static partial HGDIOBJ SelectObject(
        HDC hdc,
        HGDIOBJ h);
}
