using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class Gdi32
{
    [LibraryImport(Libraries.Gdi32)]
    internal static partial HBRUSH CreateSolidBrush(
        COLORREF color);
}
