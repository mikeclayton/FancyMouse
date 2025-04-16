using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Gets the DPI_AWARENESS_CONTEXT for the current thread.
    /// </summary>
    /// <returns>The current DPI_AWARENESS_CONTEXT for the thread.</returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getthreaddpiawarenesscontext
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial DPI_AWARENESS_CONTEXT GetThreadDpiAwarenessContext();
}
