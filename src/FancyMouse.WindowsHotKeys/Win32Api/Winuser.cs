﻿using System.Runtime.InteropServices;

namespace FancyMouse.WindowsHotKeys.Win32Api;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/
/// </remarks>
internal static class Winuser
{

    #region Keyboard and Mouse Input
    // see https://learn.microsoft.com/en-us/windows/win32/api/_inputdev/

    #region Constants

    // RegisterHotKey 
    // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_NOREPEAT = 0x4000;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;

    #endregion

    #region Functions

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
    [DllImport("user32", SetLastError = true)]
    public static extern int RegisterHotKey(
        [In] IntPtr hWnd,
        [In] int id,
        [In] uint fsModifiers,
        [In] uint vk
    );

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
    [DllImport("user32", SetLastError = true)]
    public static extern int UnregisterHotKey(
        IntPtr hWnd,
        int id
    );

    #endregion

    #endregion

    #region Windows and Messages
    // see https://learn.microsoft.com/en-us/windows/win32/api/_winmsg/

    #region Constants

    public const uint CW_USEDEFAULT = 0x80000000;

    public const int HWND_MESSAGE = -3;

    // GetSysColor
    // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsyscolor
    public const uint COLOR_WINDOW = 5;
    public const uint COLOR_BACKGROUND = 1;

    // LoadCursor
    // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadcursorw
    public const uint IDC_CROSS = 32515;

    // ShowWindow
    // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
    public const int SW_HIDE = 0;
    public const int SW_SHOWNORMAL = 1;
    public const int SW_NORMAL = 1;
    public const int SW_SHOWMINIMIZED = 2;
    public const int SW_SHOWMAXIMIZED = 3;
    public const int SW_MAXIMIZE = 3;
    public const int SW_SHOWNOACTIVATE = 4;
    public const int SW_SHOW = 5;
    public const int SW_MINIMIZE = 6;
    public const int SW_SHOWMINNOACTIVE = 7;
    public const int SW_SHOWNA = 8;
    public const int SW_RESTORE = 9;
    public const int SW_SHOWDEFAULT = 10;
    public const int SW_FORCEMINIMIZE = 11;

    // Window Styles
    // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles
    public const uint WS_OVERLAPPEDWINDOW = 0xcf0000;
    public const uint WS_VISIBLE = 0x10000000;

    // Window Class Styles
    // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-class-styles
    public const uint CS_DBLCLKS = 0x0008;
    public const uint CS_HREDRAW = 0x0002;
    public const uint CS_VREDRAW = 0x0001;

    // Cursor Notfications
    // see https://learn.microsoft.com/en-us/windows/win32/menurc/cursor-notifications
    public const uint WM_SETCURSOR = 0x0020;

    // Dialog Box Notifications
    public const uint WM_GETDLGCODE = 0x0087;

    // Keyboard Accelerator Notifications
    // see https://learn.microsoft.com/en-us/windows/win32/menurc/wm-syscommand
    public const uint WM_SYSCOMMAND = 0x0112;

    // Keyboard Input Notifications
    // see https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-activate
    public const uint WM_ACTIVATE = 0x0006;
    public const uint WM_HOTKEY = 0x0312;
    public const uint WM_KEYDOWN = 0x0100;
    public const uint WM_KEYUP = 0x0101;
    public const uint WM_KILLFOCUS = 0x0008;
    public const uint WM_SETFOCUS = 0x0007;
    public const uint WM_SYSKEYDOWN = 0x104;
    public const uint WM_SYSKEYUP = 0x0105;

    // Mouse Input Notifications
    // see https://learn.microsoft.com/en-us/windows/win32/inputdev/mouse-input-notifications
    public const uint WM_CAPTURECHANGED = 0x0215;
    public const uint WM_LBUTTONDOWN = 0x0201;
    public const uint WM_LBUTTONUP = 0x0202;
    public const uint WM_LBUTTONDBLCLK = 0x0203;
    public const uint WM_MOUSEACTIVATE = 0x0021;
    public const uint WM_MOUSEMOVE = 0x0200;
    public const uint WM_NCHITTEST = 0x0084;
    public const uint WM_NCLBUTTONDOWN = 0x00a1;
    public const uint WM_NCMOUSELEAVE = 0x2a2;
    public const uint WM_NCMOUSEMOVE = 0x00a0;

    // Painting and Drawing Messages
    // see https://learn.microsoft.com/en-us/windows/win32/gdi/wm-paint
    public const uint WM_NCPAINT = 0x0085;
    public const uint WM_PAINT = 0x000f;

    // Window Messages
    // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-messages
    public const uint WM_ERASEBKGND = 0x0014;

    // Window Notifications
    // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-notifications
    public const uint WM_ACTIVATEAPP = 0x001c;
    public const uint WM_CLOSE = 0x0010;
    public const uint WM_CREATE = 0x001;
    public const uint WM_DESTROY = 0x002;
    public const uint WM_ENTERSIZEMOVE = 0x0231;
    public const uint WM_EXITSIZEMOVE = 0x0232;
    public const uint WM_GETICON = 0x007f;
    public const uint WM_GETMINMAXINFO = 0x0024;
    public const uint WM_MOVE = 0x0003;
    public const uint WM_MOVING = 0x0216;
    public const uint WM_NCACTIVATE = 0x0086;
    public const uint WM_NCCALCSIZE = 0x0083;
    public const uint WM_NCCREATE = 0x0081;
    public const uint WM_NCDESTROY = 0x0082;
    public const uint WM_QUIT = 0x0012;
    public const uint WM_SHOWWINDOW = 0x0018;
    public const uint WM_SIZE = 0x0005;
    public const uint WM_WINDOWPOSCHANGED = 0x0047;
    public const uint WM_WINDOWPOSCHANGING = 0x0046;

    // Windows Accessibility Features
    // see https://learn.microsoft.com/en-us/windows/win32/winauto/wm-getobject
    public const uint WM_GETOBJECT = 0x003d;

    // Input Method Manager Messages
    // see https://learn.microsoft.com/en-us/windows/win32/intl/wm-ime-setcontext
    public const uint WM_IME_NOTIFY = 0x0282;
    public const uint WM_IME_SETCONTEXT = 0x281;

    #endregion

    #region Functions
    // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/#functions

    /// <summary>
    /// Creates an overlapped, pop-up, or child window with an extended window style
    /// </summary>
    /// <param name="dwExStyle"></param>
    /// <param name="lpClassName"></param>
    /// <param name="lpWindowName"></param>
    /// <param name="dwStyle"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="nWidth"></param>
    /// <param name="nHeight"></param>
    /// <param name="hWndParent"></param>
    /// <param name="hMenu"></param>
    /// <param name="hInstance"></param>
    /// <param name="lpParam"></param>
    /// <returns></returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createwindowexw
    /// </remarks>
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateWindowEx(
        int dwExStyle,
        //[MarshalAs(UnmanagedType.LPStr)]
        //string lpClassName,
        ushort lpClassName,
        [MarshalAs(UnmanagedType.LPStr)]
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam
    );

    /// <summary>
    /// Calls the default window procedure to provide default processing for any window messages that an application does not process. 
    /// </summary>
    /// <param name="hWnd">A handle to the window procedure that received the message.</param>
    /// <param name="uMsg">The message.</param>
    /// <param name="wParam">Additional message information. The content of this parameter depends on the value of the Msg parameter.</param>
    /// <param name="lParam">Additional message information. The content of this parameter depends on the value of the Msg parameter.</param>
    /// <returns>
    /// The return value is the result of the message processing and depends on the message.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-defwindowprocw
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

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
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);
    
    /// <summary>
    /// Retrieves a message from the calling thread's message queue.
    /// The function dispatches incoming sent messages until a posted message is available for retrieval.
    /// </summary>
    /// <param name="lpMsg">A pointer to an MSG structure that receives message information from the thread's message queue.</param>
    /// <param name="hWnd">A handle to the window whose messages are to be retrieved. The window must belong to the current thread.</param>
    /// <param name="wMsgFilterMin">The integer value of the lowest message value to be retrieved.</param>
    /// <param name="wMsgFilterMax">The integer value of the highest message value to be retrieved.</param>
    /// <returns>
    /// If the function retrieves a message other than WM_QUIT, the return value is nonzero.
    /// If the function retrieves the WM_QUIT message, the return value is zero.
    /// If there is an error, the return value is -1.
    /// For example, the function fails if hWnd is an invalid window handle or lpMsg is an invalid pointer.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessage
    /// </remarks>
    [DllImport("user32.dll", SetLastError = false)]
    public static extern int GetMessage(
        out MSG lpMsg,
        IntPtr hWnd,
        uint wMsgFilterMin,
        uint wMsgFilterMax
    );

    /// <summary>
    /// Dispatches incoming nonqueued messages, checks the thread message queue for a
    /// posted message, and retrieves the message (if any exist).
    /// </summary>
    /// <param name="lpMsg">A pointer to an MSG structure that receives message information.</param>
    /// <param name="hWnd">A handle to the window whose messages are to be retrieved. The window must belong to the current thread.</param>
    /// <param name="wMsgFilterMin">The value of the first message in the range of messages to be examined.</param>
    /// <param name="wMsgFilterMax">The value of the last message in the range of messages to be examined.</param>
    /// <param name="wRemoveMsg">Specifies how messages are to be handled. </param>
    /// <returns>
    /// If a message is available, the return value is nonzero.
    /// If no messages are available, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-peekmessagea
    /// </remarks>
    [DllImport("user32.dll", SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern int PeekMessage(
        out MSG lpMsg,
        IntPtr hWnd,
        uint wMsgFilterMin,
        uint wMsgFilterMax,
        uint wRemoveMsg
    );
    
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
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.U2)]
    public static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpwcx);

    /// <summary>
    /// Sets the specified window's show state.
    /// </summary>
    /// <param name="hWnd">A handle to the window.</param>
    /// <param name="nCmdShow">Controls how the window is to be shown. </param>
    /// <returns>
    /// If the window was previously visible, the return value is nonzero.
    /// If the window was previously hidden, the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern uint ShowWindow(
        IntPtr hWnd,
        int nCmdShow
    );

    /// <summary>
    /// Translates virtual-key messages into character messages.
    /// </summary>
    /// <param name="lpMsg"></param>
    /// <returns>
    /// If the message is translated (that is, a character message is posted to the thread's message queue), the return value is nonzero.
    /// If the message is WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP, the return value is nonzero, regardless of the translation.
    /// If the message is not translated (that is, a character message is not posted to the thread's message queue), the return value is zero.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-translatemessage
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern bool TranslateMessage([In] ref MSG lpMsg);

    #endregion

    #region Callback Functions
    // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/#callback-functions

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
    /// </remarks>
    public delegate IntPtr WNDPROC(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    #endregion

    #region Structures
    // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/#structures

    /// <summary>
    /// Contains message information from a thread's message queue.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-msg
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public Windef.POINT pt;
        public int lPrivate;
    }

    /// <summary>
    /// Contains window class information. It is used with the RegisterClassEx and GetClassInfoEx  functions.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-wndclassexw
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WNDCLASSEX
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public int style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    #endregion

    #endregion

}
