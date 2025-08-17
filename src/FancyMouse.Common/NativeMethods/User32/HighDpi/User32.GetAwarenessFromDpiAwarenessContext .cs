using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Retrieves the DPI_AWARENESS value from a DPI_AWARENESS_CONTEXT.
    /// </summary>
    /// <param name="value">The DPI_AWARENESS_CONTEXT you want to examine.</param>
    /// <returns>The DPI_AWARENESS. If the provided value is null or invalid, this method will return DPI_AWARENESS_INVALID.</returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getawarenessfromdpiawarenesscontext
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial DPI_AWARENESS GetAwarenessFromDpiAwarenessContext(
        DPI_AWARENESS_CONTEXT value);
}
