using FancyMouse.NativeMethods.Core;
using System.Runtime.InteropServices;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{

    [LibraryImport(Libraries.Gdi32)]
    public static partial HBITMAP CreateCompatibleBitmap(HDC hdc, int cx, int cy);

}
