using System.Runtime.InteropServices;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{
    [LibraryImport(Libraries.Gdi32)]
    internal static partial HBRUSH CreateSolidBrush(
        COLORREF color);
}
