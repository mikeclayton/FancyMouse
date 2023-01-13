using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// ReSharper disable CheckNamespace
namespace FancyMouse.WindowsHotKeys.Interop;
// ReSharper restore CheckNamespace

internal static partial class User32
{

    /// <summary>
    /// Places (posts) a message in the message queue associated with the thread that created the
    /// specified window and returns without waiting for the thread to process the message.
    ///
    /// To post a message in the message queue associated with a thread, use the PostThreadMessage function.
    /// </summary>
    /// <param name="hWnd">A handle to the window whose window procedure is to receive the message.</param>
    /// <param name="Msg">The message to be posted.</param>
    /// <param name="wParam">Additional message-specific information.</param>
    /// <param name="lParam">Additional message-specific information.</param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// To get extended error information, call GetLastError.
    /// GetLastError returns ERROR_NOT_ENOUGH_QUOTA when the limit is hit.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-postmessagew
    ///     https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/User32/Interop.PostMessage.cs
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [LibraryImport(Libraries.User32, StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint PostMessageW(
        in int hWnd,
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        in uint Msg,
        in IntPtr wParam,
        in IntPtr lParam
    );

}
