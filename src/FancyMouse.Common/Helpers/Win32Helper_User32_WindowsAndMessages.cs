using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
    public static partial class User32
    {
        /// <summary>
        /// Creates a new "message only" window.
        /// </summary>
        /// <remarks>
        /// A message-only window is a window that is not visible and does not have a user interface.
        /// It exists solely to receive and process messages.
        /// These windows are used for background tasks that need to handle messages without displaying any graphical elements to the user.
        /// </remarks>
        public static (ATOM ClassAtom, HWND Hwnd) CreateMessageOnlyWindow(string className, string windowName, WNDPROC wndProc)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(className);
            ArgumentNullException.ThrowIfNullOrEmpty(windowName);

            // see https://learn.microsoft.com/en-us/windows/win32/winmsg/using-messages-and-message-queues
            var hInstance = (HINSTANCE)Process.GetCurrentProcess().Handle;

            // register the window class
            // see https://stackoverflow.com/a/30992796/3156906
            var wndClass = new WNDCLASSEXW(
                cbSize: (UINT)WNDCLASSEXW.Size,
                style: 0,
                lpfnWndProc: wndProc,
                cbClsExtra: 0,
                cbWndExtra: 0,
                hInstance: hInstance,
                hIcon: HICON.Null,
                hCursor: HCURSOR.Null,
                hbrBackground: HBRUSH.Null,
                lpszMenuName: PCWSTR.Null,
                lpszClassName: (PCWSTR)className,
                hIconSm: HICON.Null);
            var atom = NativeMethods.User32.RegisterClassExW(
                unnamedParam1: wndClass);
            if (atom.Value == 0)
            {
                throw Win32Helper.NewWin32Exception(atom.Value, Marshal.GetLastPInvokeError());
            }

            // create the window
            // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features#message-only-windows
            //     https://devblogs.microsoft.com/oldnewthing/20171218-00/?p=97595
            //     https://stackoverflow.com/a/30992796/3156906
            var x = (LPCWSTR)className;
            var hWnd = NativeMethods.User32.CreateWindowExW(
                dwExStyle: 0,
                lpClassName: (LPCWSTR)className,
                lpWindowName: (LPCWSTR)windowName,
                dwStyle: 0,
                x: 0,
                y: 0,
                nWidth: 300,
                nHeight: 400,
                hWndParent: HWND.HWND_MESSAGE, // message-only window
                hMenu: HMENU.Null,
                hInstance: hInstance,
                lpParam: LPVOID.Null);
            if (hWnd.IsNull)
            {
                throw Win32Helper.NewWin32Exception((UINT)hWnd.Value, Marshal.GetLastPInvokeError());
            }

            return (atom, hWnd);
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

        public static LRESULT DispatchMessage(MSG lpMsg)
        {
            return NativeMethods.User32.DispatchMessageW(lpMsg);
        }

        public static BOOL GetMessage(
            LPMSG lpMsg,
            HWND hWnd,
            UINT wMsgFilterMin,
            UINT wMsgFilterMax)
        {
            return NativeMethods.User32.GetMessageW(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
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

        public static void PostMessage(HWND hWnd, MESSAGE_TYPE msg, WPARAM wParam, LPARAM lParam)
        {
            var uResult = NativeMethods.User32.PostMessageW(
                hWnd: hWnd,
                Msg: msg,
                wParam: wParam,
                lParam: lParam);
            if (!uResult)
            {
                throw Win32Helper.NewWin32Exception((UINT)uResult.Value, Marshal.GetLastPInvokeError());
            }
        }

        public static void PostThreadMessage(DWORD idThread, MESSAGE_TYPE msg, WPARAM wParam, LPARAM lParam)
        {
            var uResult = NativeMethods.User32.PostThreadMessageW(
                idThread: idThread,
                Msg: msg,
                wParam: wParam,
                lParam: lParam);
            if (!uResult)
            {
                throw Win32Helper.NewWin32Exception((UINT)uResult.Value, Marshal.GetLastPInvokeError());
            }
        }

        public static void SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, LONG dwNewLong)
        {
            // clear the last error before calling SetWindowLongW
            NativeMethods.Kernel32.SetLastError(0);

            var result = NativeMethods.User32.SetWindowLongW(hWnd, nIndex, dwNewLong);

            // failure is only if SetWindowLongW returns 0 and last error is non-zero
            var lastError = Marshal.GetLastPInvokeError();
            if ((result == 0) && (lastError != 0))
            {
                throw Win32Helper.NewWin32Exception((UINT)result.Value, Marshal.GetLastPInvokeError());
            }
        }

        public static BOOL TranslateMessage(MSG lpMsg)
        {
            return NativeMethods.User32.TranslateMessage(lpMsg);
        }
    }
}
