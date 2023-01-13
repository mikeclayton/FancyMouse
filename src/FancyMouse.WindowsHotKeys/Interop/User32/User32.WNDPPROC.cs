using System.Diagnostics.CodeAnalysis;

// ReSharper disable CheckNamespace
namespace FancyMouse.WindowsHotKeys.Interop;
// ReSharper restore CheckNamespace

internal static partial class User32
{

    /// <summary>
    /// A callback function, which you define in your application, that processes messages sent to a window.
    /// </summary>
    /// <param name="hWnd">A handle to the window. This parameter is typically named hWnd.</param>
    /// <param name="msg">The message. This parameter is typically named uMsg.</param>
    /// <param name="wParam">Additional message information. This parameter is typically named wParam.</param>
    /// <param name="lParam">Additional message information. This parameter is typically named lParam.</param>
    /// <returns>
    /// The return value is the result of the message processing, and depends on the message sent.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-wndproc
    ///     https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/User32/Interop.WndProc.cs
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public delegate IntPtr WNDPROC(
        IntPtr hWnd,
        WindowMessages msg,
        IntPtr wParam,
        IntPtr lParam
    );

}
