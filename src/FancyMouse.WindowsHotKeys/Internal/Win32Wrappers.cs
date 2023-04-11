using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;
using static FancyMouse.NativeMethods.User32;

namespace FancyMouse.WindowsHotKeys.Internal;

internal static class Win32Wrappers
{
    /// <summary>
    /// Posts a message to the message queue of the specified thread. It returns without waiting for the thread to process the message.
    /// </summary>
    /// <param name="idThread">The identifier of the thread to which the message is to be posted.</param>
    /// <param name="Msg">The type of message to be posted.</param>
    /// <param name="wParam">wParam - Additional message-specific information.</param>
    /// <param name="lParam">lParam - Additional message-specific information.</param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// To get extended error information, call GetLastError. GetLastError returns ERROR_INVALID_THREAD_ID
    /// if idThread is not a valid thread identifier, or if the thread specified by idThread does not have
    /// a message queue. GetLastError returns ERROR_NOT_ENOUGH_QUOTA when the message limit is hit.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-postmessagew
    /// </remarks>
    public static int PostThreadMessageW(
        DWORD idThread,
        [SuppressMessage("SA1313", "SA1313:ParameterNamesMustBeginWithLowerCaseLetter", Justification = "Parameter name matches Win32 api")]
        MESSAGE_TYPE Msg,
        WPARAM wParam,
        LPARAM lParam)
    {
        var result = User32.PostThreadMessageW(
            idThread, Msg, wParam, lParam);

        if (result == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"{nameof(User32.PostThreadMessageW)} failed with result {result}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error));
        }

        return result;
    }

    /// <summary>
    /// Registers a window class for subsequent use in calls to the CreateWindow or CreateWindowEx function.
    /// </summary>
    /// <param name="lpwcx">
    /// A pointer to a WNDCLASSEX structure.
    /// You must fill the structure with the appropriate class attributes before passing it to the function.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is a class atom that uniquely identifies the class being registered.
    /// This atom can only be used by the CreateWindow, CreateWindowEx, GetClassInfo, GetClassInfoEx, FindWindow,
    /// FindWindowEx, and UnregisterClass functions and the IActiveIMMap::FilterClientWindows method.
    /// If the function fails, the return value is zero.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerclassexa
    /// </remarks>
    public static ushort RegisterClassExW(
        ref User32.WNDCLASSEXW lpwcx)
    {
        var result = User32.RegisterClassExW(ref lpwcx);

        if (result == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"{nameof(User32.RegisterClassExW)} failed with result {result}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error));
        }

        return result;
    }

    /// <summary>
    /// Creates an overlapped, pop-up, or child window with an extended window style
    /// </summary>
    /// <param name="dwExStyle">
    /// The extended window style of the window being created.
    /// </param>
    /// <param name="lpClassName">
    /// A null-terminated string or a class atom created by a previous call to the RegisterClass
    /// or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the
    /// high-order word must be zero. If lpClassName is a string, it specifies the window class
    /// name. The class name can be any name registered with RegisterClass or RegisterClassEx,
    /// provided that the module that registers the class is also the module that creates the
    /// window. The class name can also be any of the predefined system class names.
    /// </param>
    /// <param name="lpWindowName">
    /// The window name. If the window style specifies a title bar, the window title pointed to by
    /// lpWindowName is displayed in the title bar. When using CreateWindow to create controls,
    /// such as buttons, check boxes, and static controls, use lpWindowName to specify the text of
    /// the control. When creating a static control with the SS_ICON style, use lpWindowName to
    /// specify the icon name or identifier. To specify an identifier, use the syntax "#num".
    /// </param>
    /// <param name="dwStyle">
    /// The style of the window being created. This parameter can be a combination of the window
    /// style values, plus the control styles indicated in the Remarks section.
    /// </param>
    /// <param name="x">
    /// The initial horizontal position of the window. For an overlapped or pop-up window, the x
    /// parameter is the initial x-coordinate of the window's upper-left corner, in screen coordinates.
    /// For a child window, x is the x-coordinate of the upper-left corner of the window relative to
    /// the upper-left corner of the parent window's client area. If x is set to CW_USEDEFAULT, the
    /// system selects the default position for the window's upper-left corner and ignores the y
    /// parameter. CW_USEDEFAULT is valid only for overlapped windows; if it is specified for a pop-up
    /// or child window, the x and y parameters are set to zero.
    /// </param>
    /// <param name="y">
    /// The initial vertical position of the window. For an overlapped or pop-up window, the y
    /// parameter is the initial y-coordinate of the window's upper-left corner, in screen coordinates.
    /// For a child window, y is the initial y-coordinate of the upper-left corner of the child window
    /// relative to the upper-left corner of the parent window's client area. For a list box y is the
    /// initial y-coordinate of the upper-left corner of the list box's client area relative to the
    /// upper-left corner of the parent window's client area.
    ///
    /// If an overlapped window is created with the WS_VISIBLE style bit set and the x parameter is
    /// set to CW_USEDEFAULT, then the y parameter determines how the window is shown.If the y
    /// parameter is CW_USEDEFAULT, then the window manager calls ShowWindow with the SW_SHOW flag
    /// after the window has been created.If the y parameter is some other value, then the window
    /// manager calls ShowWindow with that value as the nCmdShow parameter.
    /// </param>
    /// <param name="nWidth">
    /// The width, in device units, of the window. For overlapped windows, nWidth is the window's
    /// width, in screen coordinates, or CW_USEDEFAULT. If nWidth is CW_USEDEFAULT, the system
    /// selects a default width and height for the window; the default width extends from the
    /// initial x-coordinates to the right edge of the screen; the default height extends from
    /// the initial y-coordinate to the top of the icon area. CW_USEDEFAULT is valid only for
    /// overlapped windows; if CW_USEDEFAULT is specified for a pop-up or child window, the
    /// nWidth and nHeight parameter are set to zero.
    /// </param>
    /// <param name="nHeight">
    /// The height, in device units, of the window. For overlapped windows, nHeight is the window's
    /// height, in screen coordinates. If the nWidth parameter is set to CW_USEDEFAULT, the system
    /// ignores nHeight.
    /// </param>
    /// <param name="hWndParent">
    /// A handle to the parent or owner window of the window being created. To create a child
    /// window or an owned window, supply a valid window handle. This parameter is optional
    /// for pop-up windows.
    ///
    /// To create a message-only window, supply HWND_MESSAGE or a handle to an existing
    /// message-only window.
    /// </param>
    /// <param name="hMenu">
    /// A handle to a menu, or specifies a child-window identifier, depending on the window
    /// style. For an overlapped or pop-up window, hMenu identifies the menu to be used with
    /// the window; it can be NULL if the class menu is to be used. For a child window, hMenu
    /// specifies the child-window identifier, an integer value used by a dialog box control
    /// to notify its parent about events. The application determines the child-window
    /// identifier; it must be unique for all child windows with the same parent window.
    /// </param>
    /// <param name="hInstance">
    /// A handle to the instance of the module to be associated with the window.
    /// </param>
    /// <param name="lpParam">
    /// Pointer to a value to be passed to the window through the CREATESTRUCT structure
    /// (lpCreateParams member) pointed to by the lParam param of the WM_CREATE message.
    /// This message is sent to the created window by this function before it returns.
    ///
    /// If an application calls CreateWindow to create a MDI client window, lpParam should
    /// point to a CLIENTCREATESTRUCT structure.If an MDI client window calls CreateWindow
    /// to create an MDI child window, lpParam should point to a MDICREATESTRUCT structure.
    /// lpParam may be NULL if no additional data is needed.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is a handle to the new window.
    /// If the function fails, the return value is NULL.To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createwindowexw
    /// </remarks>
    public static HWND CreateWindowEx(
        User32.WINDOW_EX_STYLE dwExStyle,
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
        var result = User32.CreateWindowExW(
            dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);

        if (result.IsNull)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"{nameof(User32.CreateWindowExW)} failed with result {result}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error));
        }

        return result;
    }

    /// <summary>
    /// Defines a system-wide hot key.
    /// </summary>
    /// <param name="hWnd">
    /// A handle to the window that will receive WM_HOTKEY messages generated by the hot key
    /// If this parameter is NULL, WM_HOTKEY messages are posted to the message queue of the
    /// calling thread and must be processed in the message loop.
    /// </param>
    /// <param name="id">
    /// The identifier of the hot key. If the hWnd parameter is NULL, then the hot key is
    /// associated with the current thread rather than with a particular window. If a hot
    /// key already exists with the same hWnd and id parameters, see Remarks for the action
    /// taken.
    /// </param>
    /// <param name="fsModifiers">
    /// The keys that must be pressed in combination with the key specified by the uVirtKey
    /// parameter in order to generate the WM_HOTKEY message
    /// </param>
    /// <param name="vk">
    /// The virtual-key code of the hot key. See Virtual Key Codes.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
    /// </remarks>
    public static BOOL RegisterHotKey(
        HWND hWnd,
        int id,
        HOT_KEY_MODIFIERS fsModifiers,
        UINT vk)
    {
        var result = User32.RegisterHotKey(
            hWnd, id, fsModifiers, vk);

        if (!result)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"{nameof(User32.RegisterHotKey)} failed with result {result}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error));
        }

        return id;
    }

    /// <summary>
    /// Frees a hot key previously registered by the calling thread.
    /// </summary>
    /// <param name="hWnd">
    /// A handle to the window associated with the hot key to be freed.
    /// This parameter should be NULL if the hot key is not associated with a window.
    /// </param>
    /// <param name="id">
    /// The identifier of the hot key to be freed.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterhotkey
    /// </remarks>
    public static BOOL UnregisterHotKey(
        HWND hWnd,
        int id)
    {
        var result = User32.UnregisterHotKey(
            hWnd, id);

        if (!result)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"{nameof(User32.UnregisterHotKey)} failed with result {result}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error));
        }

        return result;
    }
}
