using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// The FillRect function fills a rectangle by using the specified brush.
    /// This function includes the left and top borders, but excludes the right
    /// and bottom borders of the rectangle.
    /// </summary>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-fillrect
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial int FillRect(
      HDC hDC,
      ref RECT lprc,
      HBRUSH hbr);
}
