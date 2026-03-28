using System.ComponentModel;
using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.Win32.Interop;

public static partial class User32
{
    public static HWND CreateWindowEx(
        WINDOW_EX_STYLE dwExStyle,
        LPCWSTR lpClassName,
        LPCWSTR lpWindowName,
        WINDOW_STYLE dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        HWND hWndParent,
        HMENU hMenu,
        HINSTANCE hInstance,
        LPVOID lpParam)
    {
        var hWnd = NativeMethods.User32.CreateWindowExW(
            dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
        if (hWnd.IsNull)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw Win32Helper.NewWin32Exception((UINT)hWnd.Value, lastError);
        }

        return hWnd;
    }

    /// <summary>
    /// Calls the default window procedure to provide default processing for any window messages that an application does not process.
    /// </summary>
    /// <param name="hWnd">A handle to the window procedure that received the message.</param>
    /// <param name="uMsg">The message.</param>
    /// <param name="wParam">wParam - Additional message information. The content of this parameter depends on the value of the Msg parameter.</param>
    /// <param name="lParam">lParam - Additional message information. The content of this parameter depends on the value of the Msg parameter.</param>
    /// <returns>
    /// The return value is the result of the message processing and depends on the message.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-defwindowprocw
    ///     https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/User32/Interop.DefWindowProc.cs
    /// </remarks>
    public static LRESULT DefWindowProc(HWND hWnd, MESSAGE_TYPE uMsg, WPARAM wParam, LPARAM lParam)
    {
        return NativeMethods.User32.DefWindowProcW(hWnd, uMsg, wParam, lParam);
    }

    public static void DestroyWindow(HWND hWnd)
    {
        var result = NativeMethods.User32.DestroyWindow(hWnd);
        if (result == 0)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }
    }

    public static LRESULT DispatchMessage(MSG lpMsg)
    {
        return NativeMethods.User32.DispatchMessageW(lpMsg);
    }

    public static HWND GetDesktopWindow()
    {
        return NativeMethods.User32.GetDesktopWindow();
    }

    public static BOOL GetMessage(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax)
    {
        var result = NativeMethods.User32.GetMessageW(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
        if (result == -1)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw Win32Helper.NewWin32Exception((UINT)result.Value, lastError);
        }

        return result;
    }

    public static int GetSystemMetrics(SYSTEM_METRICS_INDEX smIndex)
    {
        var result = NativeMethods.User32.GetSystemMetrics(smIndex);
        if (result == 0)
        {
            throw Win32Helper.NewWin32Exception((UINT)result);
        }

        return result;
    }

    /// <summary>
    /// Retrieves information about the specified window.
    /// The function also retrieves the 32-bit (DWORD) value at the specified offset into the extra window memory.
    /// </summary>
    public static LONG GetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
    {
        var result = NativeMethods.User32.GetWindowLongW(hWnd, nIndex);
        if (result != 0)
        {
            return result;
        }

        // if GetWindowLongW returns 0 it *could* be an error
        var lastError = Marshal.GetLastPInvokeError();
        if (lastError == 0)
        {
            // not an error - it's a legitimate "0" result
            return result;
        }

        throw Win32Helper.NewWin32Exception((UINT)result.Value, lastError);
    }

    public static LONG_PTR GetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
    {
        var result = NativeMethods.User32.GetWindowLongPtrW(hWnd, nIndex);
        if (result != 0)
        {
            return result;
        }

        // if GetWindowLongPtrW returns 0 it *could* be an error
        var lastError = Marshal.GetLastPInvokeError();
        if (lastError == 0)
        {
            // not an error - it's a legitimate "0" result
            return result;
        }

        throw Win32Helper.NewWin32Exception((UINT)result.Value, lastError);
    }

    public static BOOL IsWindow(HWND hWnd)
    {
        return NativeMethods.User32.IsWindow(hWnd);
    }

    public static BOOL PeekMessage(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, PEEK_MESSAGE_REMOVE_TYPE wRemoveMsg)
    {
        var result = NativeMethods.User32.PeekMessageW(
            lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
        return result;
    }

    public static void PostMessage(HWND hWnd, MESSAGE_TYPE msg, WPARAM wParam, LPARAM lParam)
    {
        var result = NativeMethods.User32.PostMessageW(
            hWnd, msg, wParam, lParam);
        if (result == 0)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw Win32Helper.NewWin32Exception((UINT)result.Value, lastError);
        }
    }

    public static void PostThreadMessage(DWORD idThread, MESSAGE_TYPE msg, WPARAM wParam, LPARAM lParam)
    {
        var result = NativeMethods.User32.PostThreadMessageW(
            idThread, msg, wParam, lParam);
        if (!result)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }
    }

    public static void PostQuitMessage(int nExitCode)
    {
        NativeMethods.User32.PostQuitMessage(nExitCode);
    }

    public static ATOM RegisterClassEx(WNDCLASSEXW unnamedParam1)
    {
        var atom = NativeMethods.User32.RegisterClassExW(unnamedParam1);
        if (atom.Value == 0)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw new Win32Exception(lastError);
        }

        return atom;
    }

    public static BOOL SetForegroundWindow(HWND hWnd)
    {
        var result = NativeMethods.User32.SetForegroundWindow(hWnd);
        return result;
    }

    public static void SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, LONG dwNewLong)
    {
        // clear the last error before calling SetWindowLongW
        NativeMethods.Kernel32.SetLastError(0);

        var result = NativeMethods.User32.SetWindowLongW(hWnd, nIndex, dwNewLong);

        // failure is only if SetWindowLongW returns 0 and last error is non-zero
        if (result == 0)
        {
            var lastError = Marshal.GetLastPInvokeError();
            if (lastError != 0)
            {
                throw Win32Helper.NewWin32Exception((UINT)result.Value, lastError);
            }
        }
    }

    public static void SetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, LONG_PTR dwNewLong)
    {
        // clear the last error before calling SetWindowLongPtrW
        NativeMethods.Kernel32.SetLastError(0);

        var result = NativeMethods.User32.SetWindowLongPtrW(hWnd, nIndex, dwNewLong);

        // failure is only if SetWindowLongPtrW returns 0 and last error is non-zero
        if (result == 0)
        {
            var lastError = Marshal.GetLastPInvokeError();
            if (lastError != 0)
            {
                throw Win32Helper.NewWin32Exception((UINT)result.Value, lastError);
            }
        }
    }

    public static BOOL TranslateMessage(MSG lpMsg)
    {
        return NativeMethods.User32.TranslateMessage(lpMsg);
    }

    public static void UnregisterClass(LPCWSTR lpClassName, HINSTANCE hInstance)
    {
        var result = NativeMethods.User32.UnregisterClassW(lpClassName, hInstance);
        if (!result)
        {
            throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
        }
    }

    public static void WaitMessage()
    {
        var result = NativeMethods.User32.WaitMessage();
        if (!result)
        {
            var lastError = Marshal.GetLastPInvokeError();
            throw Win32Helper.NewWin32Exception((UINT)result.Value, lastError);
        }
    }
}
