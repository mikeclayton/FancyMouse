using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Determines whether the current process is dots per inch (dpi) aware such that it
    /// adjusts the sizes of UI elements to compensate for the dpi setting.
    /// </summary>
    /// <returns>TRUE if the process is dpi aware; otherwise, FALSE.</returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-isprocessdpiaware
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial BOOL IsProcessDPIAware();
}
