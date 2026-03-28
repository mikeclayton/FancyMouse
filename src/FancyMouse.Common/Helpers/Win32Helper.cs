using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using FancyMouse.Win32.Interop;

using static FancyMouse.Win32.NativeMethods.Core;
using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
    internal static Win32Exception NewWin32Exception(UINT hResult, [CallerMemberName] string memberName = "")
    {
        var message = string.Join(
            Environment.NewLine,
            $"{memberName} failed with result {hResult}.");
        return new Win32Exception(message);
    }

    internal static Win32Exception NewWin32Exception(UINT hResult, int lastError, [CallerMemberName] string memberName = "")
    {
        var message = string.Join(
            Environment.NewLine,
            $"{memberName} failed with result {hResult}.",
            $"{nameof(Marshal.GetLastPInvokeError)} returned '{lastError}'",
            new Win32Exception(lastError).Message);
        return new Win32Exception(lastError, message);
    }

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
        ArgumentException.ThrowIfNullOrEmpty(className);
        ArgumentException.ThrowIfNullOrEmpty(windowName);

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
        var atom = User32.RegisterClassEx(unnamedParam1: wndClass);

        // create the window
        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features#message-only-windows
        //     https://devblogs.microsoft.com/oldnewthing/20171218-00/?p=97595
        //     https://stackoverflow.com/a/30992796/3156906
        var hWnd = User32.CreateWindowEx(
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

        return (atom, hWnd);
    }
}
