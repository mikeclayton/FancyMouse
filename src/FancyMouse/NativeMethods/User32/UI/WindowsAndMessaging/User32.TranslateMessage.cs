using System.Runtime.InteropServices;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Translates virtual-key messages into character messages.
    /// </summary>
    /// <returns>
    /// If the message is translated (that is, a character message is posted to the thread's message queue), the return value is nonzero.
    /// If the message is WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP, the return value is nonzero, regardless of the translation.
    /// If the message is not translated (that is, a character message is not posted to the thread's message queue), the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-translatemessage
    ///     https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/User32/Interop.TranslateMessage.cs
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial BOOL TranslateMessage(
        MSG lpMsg);
}
