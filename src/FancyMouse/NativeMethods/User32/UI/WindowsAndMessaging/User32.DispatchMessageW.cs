using System.Runtime.InteropServices;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    /// <summary>
    /// Dispatches a message to a window procedure. It is typically used to dispatch a message retrieved by the GetMessage function.
    /// </summary>
    /// <param name="lpmsg">A pointer to a structure that contains the message.</param>
    /// <returns>
    /// The return value specifies the value returned by the window procedure.
    /// Although its meaning depends on the message being dispatched, the return value generally is ignored.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-dispatchmessage
    ///     https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/User32/Interop.DispatchMessage.cs
    /// </remarks>
    [LibraryImport(Libraries.User32)]
    internal static partial LRESULT DispatchMessageW(
        MSG lpmsg);
}
