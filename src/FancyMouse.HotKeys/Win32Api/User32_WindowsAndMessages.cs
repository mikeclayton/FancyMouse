using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FancyMouse.HotKeys.Win32Api;

internal static partial class User32
{
    internal static unsafe Win32Result<LRESULT> DefWindowProc(
        HWND hWnd,
#pragma warning disable SA1313 // Parameter should begin with lower-case letter
        uint Msg,
#pragma warning restore SA1313 // Parameter should begin with lower-case letter
        WPARAM wParam,
        LPARAM lParam)
    {
        // The return value is the result of the message processing and depends on the message.
        return PInvoke.DefWindowProc(hWnd, Msg, wParam, lParam)
            .AlwaysSucceeds();
    }

    internal static Win32Result<LRESULT> DispatchMessage(in MSG lpMsg)
    {
        // The return value specifies the value returned by the window procedure.
        return PInvoke.DispatchMessage(lpMsg)
            .AlwaysSucceeds();
    }

    internal static Win32Result<BOOL> GetMessage(out MSG lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
    {
        // If the function retrieves a message other than WM_QUIT, the return value is nonzero.
        // If the function retrieves the WM_QUIT message, the return value is zero.
        // If there is an error, the return value is -1. For example, the function fails if hWnd
        // is an invalid window handle or lpMsg is an invalid pointer. To get extended error
        // information, call GetLastError.
        return PInvoke.GetMessage(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax)
            .SuccessIsNotSentinel((BOOL)(-1))
            .UsesLastError();
    }

    internal static Win32Result<BOOL> PostMessage(
        HWND hWnd,
#pragma warning disable SA1313 // Parameter should begin with lower-case letter
        uint Msg,
#pragma warning restore SA1313 // Parameter should begin with lower-case letter
        WPARAM wParam,
        LPARAM lParam)
    {
        // If the function succeeds, the return value is nonzero.
        // If the function fails, the return value is zero.
        // To get extended error information, call GetLastError.
        return PInvoke.PostMessage(hWnd, Msg, wParam, lParam)
            .SuccessIsNonZero()
            .UsesLastError();
    }

    internal static Win32Result<BOOL> TranslateMessage(in MSG lpMsg)
    {
        // If the message is translated (that is, a character message is posted to the thread's message queue), the return value is nonzero.
        // If the message is WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP, the return value is nonzero, regardless of the translation.
        // If the message is not translated (that is, a character message is not posted to the thread's message queue), the return value is zero.
        return PInvoke.TranslateMessage(lpMsg)
            .AlwaysSucceeds();
    }
}
