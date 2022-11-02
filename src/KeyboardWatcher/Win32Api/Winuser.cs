using System.Runtime.InteropServices;

namespace KeyboardWatcher.Win32Api;

internal static class Winuser
{

    #region Constants

    public const uint PM_REMOVE = 0x0001;

    // Event Constants
    // see https://learn.microsoft.com/en-us/windows/win32/winauto/event-constants
    public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

    // Window Notifications
    // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-notifications
    public const uint WM_QUIT = 0x0012;

    #endregion

    #region Structures

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
        public Point pt;
        public int lPrivate;
    }

    #endregion

    #region Methods

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

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
    public static extern bool PeekMessage(
        out MSG lpMsg,
        IntPtr hWnd,
        uint wMsgFilterMin,
        uint wMsgFilterMax,
        uint wRemoveMsg
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

    #region SetWindowsHookEx

    // SetWindowsHookEx hook ids
    // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
    public const int WH_KEYBOARD = 2;
    public const int WH_KEYBOARD_LL = 13;

    public const int HC_ACTION = 0;

    // Keyboard Input Notifications
    // see https://learn.microsoft.com/en-gb/windows/win32/inputdev/keyboard-input-notifications
    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;
    public const int WM_SYSKEYDOWN = 0x0104;
    public const int WM_SYSKEYUP = 0x0105;

    /// <summary>
    /// Contains information about a low-level keyboard input event.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-gb/windows/win32/api/winuser/ns-winuser-kbdllhookstruct
    /// </remarks>
    public struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    /// <summary>
    /// An application-defined or library-defined callback function used with the SetWindowsHookEx function. 
    /// </summary>
    /// <param name="code">A code the hook procedure uses to determine how to process the message.</param>
    /// <param name="wParam">The virtual-key code of the key that generated the keystroke message.</param>
    /// <param name="lParam">The repeat count, scan code, extended-key flag, context code, previous key-state flag, and transition-state flag.</param>
    /// <returns></returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644984(v=vs.85)
    /// </remarks>
    public delegate int KeyboardProc(
        int code,
        int wParam,
        int lParam
    );

    /// <summary>
    /// An application-defined or library-defined callback function used with the SetWindowsHookEx function. The system calls this function every time a new keyboard input event is about to be posted into a thread input queue.
    /// </summary>
    /// <param name="nCode">A code the hook procedure uses to determine how to process the message.</param>
    /// <param name="wParam">The identifier of the keyboard message. </param>
    /// <param name="lParam">A pointer to a KBDLLHOOKSTRUCT structure.</param>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)
    /// </remarks>
    public delegate int LowLevelKeyboardProc(
        int nCode,
        int wParam,
        KBDLLHOOKSTRUCT lParam
    );

    /// <summary>
    /// Installs an application-defined hook procedure into a hook chain.
    /// </summary>
    /// <param name="idHook">The type of hook procedure to be installed.</param>
    /// <param name="lpfn">A pointer to the hook procedure.</param>
    /// <param name="hmod">A handle to the DLL containing the hook procedure pointed to by the lpfn parameter.</param>
    /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated.</param>
    /// <returns>
    /// If the function succeeds, the return value is the handle to the hook procedure.
    /// If the function fails, the return value is NULL.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// If the function succeeds, the return value is the handle to the hook procedure.
    /// If the function fails, the return value is NULL.
    /// To get extended error information, call GetLastError.
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
    /// </remarks>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowsHookEx(
        int idHook,
        KeyboardProc lpfn,
        IntPtr hmod,
        int dwThreadId
    );

    /// <summary>
    /// Installs an application-defined hook procedure into a hook chain.
    /// </summary>
    /// <param name="idHook">The type of hook procedure to be installed.</param>
    /// <param name="lpfn">A pointer to the hook procedure.</param>
    /// <param name="hmod">A handle to the DLL containing the hook procedure pointed to by the lpfn parameter.</param>
    /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated.</param>
    /// <returns>
    /// If the function succeeds, the return value is the handle to the hook procedure.
    /// If the function fails, the return value is NULL.
    /// To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
    /// </remarks>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowsHookEx(
        int idHook,
        LowLevelKeyboardProc lpfn,
        IntPtr hmod,
        int dwThreadId
    );

    #endregion

}
