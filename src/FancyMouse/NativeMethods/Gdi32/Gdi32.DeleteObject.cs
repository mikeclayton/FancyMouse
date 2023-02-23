using System.Runtime.InteropServices;
using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    /// <summary>
    /// The DeleteObject function deletes a logical pen, brush, font, bitmap, region, or palette,
    /// freeing all system resources associated with the object. After the object is deleted,
    /// the specified handle is no longer valid.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the specified handle is not valid or is currently selected into a DC, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-deleteobject
    /// </remarks>
    [LibraryImport(Libraries.Gdi32)]
    public static partial BOOL DeleteObject(
        HGDIOBJ ho);
}
