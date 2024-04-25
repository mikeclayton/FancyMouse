using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Sends the specified message to a window or windows. The SendMessage function calls the window
    /// procedure for the specified window and does not return until the window procedure has
    /// processed the message.
    ///
    /// To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage
    /// function. To post a message to a thread's message queue and return immediately, use the
    /// PostMessage or PostThreadMessage function.
    /// </summary>
    /// <param name="hWnd">A handle to the window whose window procedure is to receive the message.</param>
    /// <param name="Msg">The message to be sent.</param>
    /// <param name="wParam">wParam - Additional message-specific information.</param>
    /// <param name="lParam">lParam - Additional message-specific information.</param>
    /// <returns>
    /// The return value specifies the result of the message processing; it depends
    /// on the message sent.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessage
    /// </remarks>
    [LibraryImport(Libraries.User32, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial LRESULT SendMessageW(
        HWND hWnd,
        [SuppressMessage("SA1313", "SA1313:ParameterNamesMustBeginWithLowerCaseLetter", Justification = "Parameter name matches Win32 api")]
        MESSAGE_TYPE Msg,
        WPARAM wParam,
        LPARAM lParam);
}
