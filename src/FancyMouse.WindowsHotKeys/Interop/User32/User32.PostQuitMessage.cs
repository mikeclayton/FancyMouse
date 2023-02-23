using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FancyMouse.WindowsHotKeys.Interop;

internal static partial class User32
{
    /// <summary>
    /// Indicates to the system that a thread has made a request to terminate (quit).
    /// It is typically used in response to a WM_DESTROY message.
    /// </summary>
    /// <param name="nExitCode">The application exit code. This value is used as the wParam parameter of the WM_QUIT message.</param>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-postquitmessage
    ///     https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/User32/Interop.PostQuitMessage.cs
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    public static partial void PostQuitMessage(
        int nExitCode);
}
