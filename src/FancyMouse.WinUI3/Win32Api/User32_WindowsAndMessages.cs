using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FancyMouse.WinUI3.Win32Api;

internal static partial class User32
{
    internal static Win32Result<int> GetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
    {
        var returnValue = PInvoke.GetWindowLong(hWnd, nIndex);
        var lastError = Marshal.GetLastPInvokeError();

        // If the function succeeds, the return value is the requested value.
        // If the function fails, the return value is zero.
        // To get extended error information, call GetLastError.
        //
        // GetWindowLong has been known to legitimately return zero (e.g. no styles set) with
        // GetLastError also returning zero to indicate success - so a zero result is only a
        // real failure when GetLastError also reports one.
        return new Win32Result<int>(
            nameof(PInvoke.GetWindowLong),
            returnValue,
            isSuccess: (returnValue != 0) || (lastError == 0),
            useLastError: true,
            lastError);
    }

    internal static Win32Result<HWND> SetActiveWindow(HWND hWnd)
    {
        var returnValue = PInvoke.SetActiveWindow(hWnd);
        var lastError = Marshal.GetLastPInvokeError();

        // If the function succeeds, the return value is the handle to the window that was
        // previously active.
        // If the hWnd parameter is invalid or the window is not attached to the calling
        // thread's message queue, the return value is NULL.
        // To get extended error information, call GetLastError.
        //
        // a NULL return is also the legitimate result when there simply was no previously
        // active window (e.g. the very first call in a session) - the docs don't separate
        // that from the real failure cases, so treat NULL as a real failure only when
        // GetLastError also reports one.
        return new Win32Result<HWND>(
            nameof(PInvoke.SetActiveWindow),
            returnValue,
            isSuccess: !returnValue.IsNull || (lastError == 0),
            useLastError: true,
            lastError);
    }

    internal static Win32Result<HWND> SetFocus(HWND hWnd)
    {
        var returnValue = PInvoke.SetFocus(hWnd);
        var lastError = Marshal.GetLastPInvokeError();

        // If the function succeeds, the return value is the handle to the window that previously had the keyboard focus.
        // If the hWnd parameter is invalid or the window is not attached to the calling thread's message queue, the return value is NULL.
        // To get extended error information, call GetLastError function.
        return new Win32Result<HWND>(
            nameof(PInvoke.SetFocus),
            returnValue,
            isSuccess: !returnValue.IsNull || (lastError == 0), // IsNull is only an error if lastError is also non-zero
            useLastError: true,
            lastError);
    }

    internal static Win32Result<BOOL> SetForegroundWindow(HWND hWnd)
    {
        // If the window was brought to the foreground, the return value is nonzero.
        // If the window was not brought to the foreground, the return value is zero.
        return PInvoke.SetForegroundWindow(hWnd)
            .SuccessIsNonZero();
    }

    internal static Win32Result<int> SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, int dwNewLong)
    {
        var returnValue = PInvoke.SetWindowLong(hWnd, nIndex, dwNewLong);
        var lastError = Marshal.GetLastPInvokeError();

        // If the function succeeds, the return value is the previous value of the specified offset.
        // If the function fails, the return value is zero.
        // To get extended error information, call GetLastError.
        //
        // SetWindowLong has been known to legitimately return zero (e.g. no previous value) with
        // GetLastError also returning zero to indicate success - so a zero result is only a
        // real failure when GetLastError also reports one.
        return new Win32Result<int>(
            nameof(PInvoke.SetWindowLong),
            returnValue,
            isSuccess: (returnValue != 0) || (lastError == 0),
            useLastError: true,
            lastError);
    }

    internal static Win32Result<BOOL> ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow)
    {
        // If the window was previously visible, the return value is nonzero.
        // If the window was previously hidden, the return value is zero.
        return PInvoke.ShowWindow(hWnd, nCmdShow)
            .AlwaysSucceeds();
    }
}
