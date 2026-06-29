using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FancyMouse.Common.Win32Api;

internal static partial class User32
{
    internal static unsafe Win32Result<HWND> CreateWindowEx(
        WINDOW_EX_STYLE dwExStyle,
        [Optional] string lpClassName,
        [Optional] string lpWindowName,
        WINDOW_STYLE dwStyle,
#pragma warning disable SA1313 // Parameter should begin with lower-case letter
        int X,
        int Y,
#pragma warning restore SA1313 // Parameter should begin with lower-case letter
        int nWidth,
        int nHeight,
        [Optional] HWND hWndParent,
        [Optional] SafeHandle? hMenu,
        [Optional] SafeHandle? hInstance,
        [Optional] void* lpParam)
    {
        // If the function succeeds, the return value is a handle to the new window.
        // If the function fails, the return value is NULL.
        // To get extended error information, call GetLastError.
        return PInvoke.CreateWindowEx(dwExStyle, lpClassName, lpWindowName, dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam)
            .SuccessIsNotNull()
            .UsesLastError();
    }

    internal static Win32Result<LRESULT> DefWindowProc(
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

    internal static Win32Result<int> GetSystemMetrics(SYSTEM_METRICS_INDEX nIndex)
    {
        // If the function succeeds, the return value is the requested system metric or configuration setting.
        // If the function fails, the return value is 0.
        // GetLastError does not provide extended error information.
        return PInvoke.GetSystemMetrics(nIndex)
            .SuccessIsNonZero();
    }

    internal static Win32Result<ushort> RegisterClassEx(in WNDCLASSEXW param0)
    {
        // If the function succeeds, the return value is a class atom that uniquely identifies the class being registered.
        // If the function fails, the return value is zero.
        // To get extended error information, call GetLastError.
        return PInvoke.RegisterClassEx(param0)
            .SuccessIsNonZero()
            .UsesLastError();
    }

    internal static Win32Result<BOOL> SetForegroundWindow(HWND hWnd)
    {
        return PInvoke.SetForegroundWindow(hWnd)
            .AlwaysSucceeds();
    }
}
