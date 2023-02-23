using System.Runtime.InteropServices;

namespace FancyMouse.WindowsHotKeys.Interop;

internal static partial class User32
{
    /// <summary>
    /// Frees a hot key previously registered by the calling thread.
    /// </summary>
    /// <param name="hWnd">
    /// A handle to the window associated with the hot key to be freed.
    /// This parameter should be NULL if the hot key is not associated with a window.
    /// </param>
    /// <param name="id">
    /// The identifier of the hot key to be freed.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterhotkey
    /// </remarks>
    [LibraryImport(Libraries.User32, SetLastError = true)]
    public static partial int UnregisterHotKey(
        IntPtr hWnd,
        int id);
}
