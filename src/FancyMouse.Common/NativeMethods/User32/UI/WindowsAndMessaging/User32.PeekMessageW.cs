using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Dispatches incoming nonqueued messages, checks the thread message queue for a
    /// posted message, and retrieves the message (if any exist).
    /// </summary>
    /// <param name="lpMsg">A pointer to an MSG structure that receives message information.</param>
    /// <param name="hWnd">A handle to the window whose messages are to be retrieved. The window must belong to the current thread.</param>
    /// <param name="wMsgFilterMin">The value of the first message in the range of messages to be examined.</param>
    /// <param name="wMsgFilterMax">The value of the last message in the range of messages to be examined.</param>
    /// <param name="wRemoveMsg">Specifies how messages are to be handled. </param>
    /// <returns>
    /// If a message is available, the return value is nonzero.
    /// If no messages are available, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-peekmessagew
    /// </remarks>
    [LibraryImport(Libraries.User32, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial BOOL PeekMessageW(
        out LPMSG lpMsg,
        HWND hWnd,
        UINT wMsgFilterMin,
        UINT wMsgFilterMax,
        PEEK_MESSAGE_REMOVE_TYPE wRemoveMsg);
}
