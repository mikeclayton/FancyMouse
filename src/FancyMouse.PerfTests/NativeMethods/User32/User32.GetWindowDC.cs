using FancyMouse.PerfTests.NativeMethods.Core;
using System.Runtime.InteropServices;

namespace FancyMouse.PerfTests.NativeMethods;

internal static partial class User32
{

    [LibraryImport(Libraries.User32)]
    public static partial HDC GetWindowDC(HWND hWnd);

}
