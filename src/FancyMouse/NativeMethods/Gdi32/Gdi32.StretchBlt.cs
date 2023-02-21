using FancyMouse.NativeMethods.Core;
using System.Runtime.InteropServices;

namespace FancyMouse.NativeMethods;

internal static partial class Gdi32
{

    [LibraryImport(Libraries.Gdi32)]
    public static partial BOOL StretchBlt(
      HDC hdcDest,
      int xDest,
      int yDest,
      int wDest,
      int hDest,
      HDC hdcSrc,
      int xSrc,
      int ySrc,
      int wSrc,
      int hSrc,
      ROP_CODE rop
    );

}
