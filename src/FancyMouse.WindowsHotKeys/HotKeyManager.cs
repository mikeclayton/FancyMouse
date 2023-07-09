using System.Diagnostics;
using System.Runtime.InteropServices;
using FancyMouse.NativeMethods;
using FancyMouse.WindowsHotKeys.Internal;
using static FancyMouse.NativeMethods.Core;
using static FancyMouse.NativeMethods.User32;

namespace FancyMouse.WindowsHotKeys;

/// <remarks>
/// See https://stackoverflow.com/a/3654821/3156906
///     https://learn.microsoft.com/en-us/archive/msdn-magazine/2007/june/net-matters-handling-messages-in-console-apps
///     https://www.codeproject.com/Articles/5274425/Understanding-Windows-Message-Queues-for-the-Cshar
/// </remarks>
public sealed class HotKeyManager
{
    private int _id;

    public event EventHandler<HotKeyEventArgs>? HotKeyPressed;

    public HotKeyManager(Keystroke hotkey)
    {
        this.HotKey = hotkey ?? throw new ArgumentNullException(nameof(hotkey));

        // cache the window proc delegate so it doesn't get garbage-collected
        this.WndProc = this.WindowProc;
        this.HWnd = HWND.Null;
    }

    public Keystroke HotKey
    {
        get;
    }

    private WNDPROC WndProc
    {
        get;
    }

    private HWND HWnd
    {
        get;
        set;
    }

    private MessageLoop? MessageLoop
    {
        get;
        set;
    }

    private LRESULT WindowProc(HWND hWnd, MESSAGE_TYPE msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case MESSAGE_TYPE.WM_HOTKEY:
                // https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
                // https://stackoverflow.com/a/47831305/3156906
                var param = (uint)lParam.Value.ToInt64();
                var key = (Keys)((param & 0xffff0000) >> 16);
                var modifiers = (KeyModifiers)(param & 0x0000ffff);
                var e = new HotKeyEventArgs(key, modifiers);
                this.OnHotKeyPressed(e);
                break;
        }

        return User32.DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    public void Start()
    {
        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/using-messages-and-message-queues
        var hInstance = Process.GetCurrentProcess().Handle;

        // see https://stackoverflow.com/a/30992796/3156906
        var wndClass = new WNDCLASSEXW(
            cbSize: (uint)Marshal.SizeOf(typeof(WNDCLASSEXW)),
            style: 0,
            lpfnWndProc: this.WndProc,
            cbClsExtra: 0,
            cbWndExtra: 0,
            hInstance: hInstance,
            hIcon: HICON.Null,
            hCursor: HCURSOR.Null,
            hbrBackground: HBRUSH.Null,
            lpszMenuName: PCWSTR.Null,
            lpszClassName: "FancyMouseMessageClass",
            hIconSm: HICON.Null);

        // wndClassAtom
        _ = Win32Wrappers.RegisterClassExW(
            unnamedParam1: wndClass);

        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features#message-only-windows
        //     https://devblogs.microsoft.com/oldnewthing/20171218-00/?p=97595
        //     https://stackoverflow.com/a/30992796/3156906
        this.HWnd = Win32Wrappers.CreateWindowEx(
            dwExStyle: 0,
            lpClassName: "FancyMouseMessageClass",
            lpWindowName: "FancyMouseMessageWindow",
            dwStyle: 0,
            x: 0,
            y: 0,
            nWidth: 300,
            nHeight: 400,
            hWndParent: HWND.HWND_MESSAGE, // message-only window
            hMenu: HMENU.Null,
            hInstance: hInstance,
            lpParam: LPVOID.Null);

        this.MessageLoop = new MessageLoop(
            name: "FancyMouseMessageLoop");

        this.MessageLoop.Start();

        _ = Win32Wrappers.RegisterHotKey(
            hWnd: this.HWnd,
            id: Interlocked.Increment(ref _id),
            fsModifiers: (HOT_KEY_MODIFIERS)this.HotKey.Modifiers,
            vk: (uint)this.HotKey.Key);
    }

    public void Stop()
    {
        _ = Win32Wrappers.UnregisterHotKey(
            hWnd: this.HWnd,
            id: this._id);
        this.MessageLoop?.Stop();
    }

    private void OnHotKeyPressed(HotKeyEventArgs e)
    {
        this.HotKeyPressed?.Invoke(null, e);
    }
}
