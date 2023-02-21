using FancyMouse.PerfTests.NativeMethods.Core;
using System.Runtime.InteropServices;

namespace FancyMouse.PerfTests.NativeMethods;

internal static partial class Gdi32
{

    [LibraryImport(Libraries.Gdi32)]
    public static partial HBITMAP CreateCompatibleBitmap(HDC hdc, int cx, int cy);

}
