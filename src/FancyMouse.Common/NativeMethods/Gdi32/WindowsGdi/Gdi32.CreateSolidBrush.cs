using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class Gdi32
{
    /// <summary>
    /// The CreateSolidBrush function creates a logical brush that has the specified solid color.
    /// </summary>
    /// <param name="color">The color of the brush. To create a COLORREF color value, use the RGB macro.</param>
    /// <returns>
    /// If the function succeeds, the return value identifies a logical brush.
    /// If the function fails, the return value is NULL.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createsolidbrush
    /// </remarks>
    [LibraryImport(Libraries.Gdi32)]
    internal static partial HBRUSH CreateSolidBrush(
        COLORREF color);
}
