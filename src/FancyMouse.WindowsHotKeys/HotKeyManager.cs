using System.Diagnostics;
using System.Runtime.InteropServices;
using FancyMouse.WindowsHotKeys.Internal;
using FancyMouse.WindowsHotKeys.Interop;

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

        // cache a window proc delegate so doesn't get garbage-collected
        this.WndProc = this.WindowProc;
        this.HWnd = IntPtr.Zero;
    }

    public Keystroke HotKey
    {
        get;
    }

    private User32.WNDPROC WndProc
    {
        get;
    }

    private IntPtr HWnd
    {
        get;
        set;
    }

    private MessageLoop? MessageLoop
    {
        get;
        set;
    }

    private IntPtr WindowProc(IntPtr hWnd, User32.WindowMessages msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case User32.WindowMessages.WM_HOTKEY:
                // https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
                // https://stackoverflow.com/a/47831305/3156906
                var param = (uint)lParam.ToInt64();
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
        var wndClass = new User32.WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf(typeof(User32.WNDCLASSEXW)),
            hInstance = hInstance,
            lpszClassName = "FancyMouseMessageClass",
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(this.WndProc),
        };

        // wndClassAtom
        _ = Win32Wrappers.RegisterClassExW(
            lpwcx: ref wndClass);

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
            hWndParent: User32.HWND_MESSAGE, // message-only window
            hMenu: IntPtr.Zero,
            hInstance: hInstance,
            lpParam: IntPtr.Zero);

        this.MessageLoop = new MessageLoop(
            name: "FancyMouseMessageLoop",
            isBackground: true);

        this.MessageLoop.Run();

        _ = Win32Wrappers.RegisterHotKey(
            hWnd: this.HWnd,
            id: Interlocked.Increment(ref _id),
            fsModifiers: (User32.RegisterHotKeyModifiers)this.HotKey.Modifiers,
            vk: (uint)this.HotKey.Key);
    }

    public void Stop()
    {
        _ = Win32Wrappers.UnregisterHotKey(
            hWnd: this.HWnd,
            id: this._id);
        this.MessageLoop?.Exit();
    }

    private void OnHotKeyPressed(HotKeyEventArgs e)
    {
        this.HotKeyPressed?.Invoke(null, e);
    }
}
