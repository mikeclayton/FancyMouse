using System.Runtime.InteropServices;
using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    [LibraryImport(Libraries.Gdi32)]
    public static partial HBRUSH CreateSolidBrush(
        COLORREF color);
}
