using FancyMouse.KeyboardWatcher.Win32Api;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FancyMouse.KeyboardWatcher;

public sealed class KeyboardWatcher
{

    #region Events

    public event KeyEventHandler KeyDown;
    public event KeyEventHandler KeyUp;

    private void OnKeyDown(KeyEventArgs e)
    {
        this.KeyDown?.Invoke(this, e);
    }

    private void OnKeyUp(KeyEventArgs e)
    {
        this.KeyUp?.Invoke(this, e);
    }

    #endregion

    #region Constructors

    public KeyboardWatcher()
    {
        this.Alt = false;
        this.Control = false;
        this.Shift = false;
        this.Callback = new Winuser.LowLevelKeyboardProc(
            this.LowLevelKeyboardCallback
        );
    }

    #endregion

    #region Properties

    public bool Alt
    {
        get;
        private set;
    }

    public bool Control
    {
        get;
        private set;
    }

    public bool Shift
    {
        get;
        private set;
    }

    private Winuser.LowLevelKeyboardProc Callback
    {
        get;
        set;
    }

    private int? HookProc
    {
        get;
        set;
    }

    #endregion

    #region Methods

    public void Start()
    {

        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/using-hooks

        if (this.HookProc != null)
        {
            throw new InvalidOperationException("watcher already started");
        }

        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule ?? throw new InvalidOperationException();

        var moduleHandle = Kernel32.GetModuleHandle(module.ModuleName);
        if (moduleHandle == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException
                ($"{nameof(Kernel32.GetModuleHandle)} failed with result {moduleHandle}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error)
            );
        }

        var apiResult = Winuser.SetWindowsHookEx(
            idHook: Winuser.WH_KEYBOARD_LL,
            lpfn: this.Callback,
            hmod: moduleHandle,
            dwThreadId: 0
        );
        if (apiResult == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException
                ($"{nameof(Winuser.SetWindowsHookEx)} failed with result {apiResult}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error)
            );
        }
        this.HookProc = apiResult;

    }

    public void Stop()
    {

        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/using-hooks

        if (this.HookProc == null)
        {
            throw new InvalidOperationException("watcher not started");
        }

        var apiResult = Winuser.UnhookWindowsHookEx(
            this.HookProc.Value
        );
        if (apiResult == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException
                ($"{nameof(Winuser.UnhookWindowsHookEx)} failed with result {apiResult}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error)
            );
        }

        this.HookProc = null;

    }

    private int LowLevelKeyboardCallback(int nCode, int wParam, Winuser.KBDLLHOOKSTRUCT lParam)
    {

        // see https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985(v=vs.85)

        var hhk = this.HookProc ?? -1;

        // If nCode is less than zero, the hook procedure must pass the message to the
        // CallNextHookEx function without further processing and should return the
        // value returned by CallNextHookEx. 
        if (nCode < 0)
        {
            return Winuser.CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        if (nCode != Winuser.HC_ACTION)
        {
            return Winuser.CallNextHookEx(hhk, nCode, wParam, lParam);
        }

        var virtualKey = (Keys)lParam.vkCode;
        switch (wParam)
        {
            case Winuser.WM_KEYDOWN:
                switch (virtualKey)
                {
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        this.Control = true;
                        break;
                    case Keys.LMenu:
                        this.Alt = true;
                        break;
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        this.Shift = true;
                        break;
                    default:
                        var args = new KeyEventArgs(
                            virtualKey |
                            (this.Control ? Keys.Control : Keys.None) |
                            (this.Alt ? Keys.Alt : Keys.None) |
                            (this.Shift ? Keys.Shift : Keys.None)
                        );
                        this.OnKeyDown(args);
                        break;
                }
                break;
            case Winuser.WM_KEYUP:
                switch (virtualKey)
                {
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        this.Control = false;
                        break;
                    case Keys.LMenu:
                        this.Alt = false;
                        break;
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        this.Shift = false;
                        break;
                    default:
                        var args = new KeyEventArgs(
                            virtualKey |
                            (this.Control ? Keys.Control : Keys.None) |
                            (this.Alt ? Keys.Alt : Keys.None) |
                            (this.Shift ? Keys.Shift : Keys.None)
                        );
                        this.OnKeyUp(args);
                        break;
                }
                break;
            case Winuser.WM_SYSKEYDOWN:
                switch (virtualKey)
                {
                    case Keys.LMenu:
                        this.Alt = true;
                        break;
                }
                break;
            case Winuser.WM_SYSKEYUP:
                break;
            default:
                break;
        }

        return Winuser.CallNextHookEx(hhk, nCode, wParam, lParam);

    }

    #endregion

}
