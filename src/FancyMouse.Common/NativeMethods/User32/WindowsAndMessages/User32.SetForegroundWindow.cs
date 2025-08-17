using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Brings the thread that created the specified window into the foreground and activates the window.
    /// Keyboard input is directed to the window, and various visual cues are changed for the user. The
    /// system assigns a slightly higher priority to the thread that created the foreground window than
    /// it does to other threads.
    /// </summary>
    /// <returns>
    /// If the window was brought to the foreground, the return value is nonzero.
    /// If the window was not brought to the foreground, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow
    /// </remarks>
    [LibraryImport(Libraries.User32, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial BOOL SetForegroundWindow(HWND hWnd);
}
