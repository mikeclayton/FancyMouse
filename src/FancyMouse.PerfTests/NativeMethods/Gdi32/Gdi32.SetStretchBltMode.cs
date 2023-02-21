using FancyMouse.PerfTests.NativeMethods.Core;
using System.Runtime.InteropServices;

namespace FancyMouse.PerfTests.NativeMethods;

internal static partial class Gdi32
{

    [LibraryImport(Libraries.Gdi32)]
    public static partial int SetStretchBltMode(
        HDC hdc,
        STRETCH_BLT_MODE mode
    );

}
