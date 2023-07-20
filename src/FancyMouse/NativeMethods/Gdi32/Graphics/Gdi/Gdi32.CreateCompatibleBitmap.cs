using System.Runtime.InteropServices;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    /// <summary>
    /// The CreateCompatibleBitmap function creates a bitmap compatible with the device that is associated with the specified device context.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is a handle to the compatible bitmap (DDB).
    /// If the function fails, the return value is NULL.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createcompatiblebitmap
    /// </remarks>
    [LibraryImport(Libraries.Gdi32)]
    internal static partial HBITMAP CreateCompatibleBitmap(
        HDC hdc,
        int cx,
        int cy);
}
