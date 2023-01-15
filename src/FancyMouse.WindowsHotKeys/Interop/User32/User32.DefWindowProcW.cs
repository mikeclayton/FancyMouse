using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ReSharper disable CheckNamespace
namespace FancyMouse.WindowsHotKeys.Interop;
// ReSharper restore CheckNamespace

internal static partial class User32
{

    /// <summary>
    /// Calls the default window procedure to provide default processing for any window messages that an application does not process. 
    /// </summary>
    /// <param name="hWnd">A handle to the window procedure that received the message.</param>
    /// <param name="uMsg">The message.</param>
    /// <param name="wParam">Additional message information. The content of this parameter depends on the value of the Msg parameter.</param>
    /// <param name="lParam">Additional message information. The content of this parameter depends on the value of the Msg parameter.</param>
    /// <returns>
    /// The return value is the result of the message processing and depends on the message.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-defwindowprocw
    ///     https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/User32/Interop.DefWindowProc.cs
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [LibraryImport(Libraries.User32)]
    public static partial IntPtr DefWindowProcW(
        IntPtr hWnd,
        WindowMessages uMsg,
        IntPtr wParam,
        IntPtr lParam
    );

}
