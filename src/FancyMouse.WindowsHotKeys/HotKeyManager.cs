using FancyMouse.WindowsHotKeys.Internal;
using FancyMouse.WindowsHotKeys.Win32Api;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace FancyMouse.WindowsHotKeys;

/// <summary>
///
/// </summary>
/// <remarks>
/// See https://stackoverflow.com/a/3654821/3156906
///     https://learn.microsoft.com/en-us/archive/msdn-magazine/2007/june/net-matters-handling-messages-in-console-apps
///     https://www.codeproject.com/Articles/5274425/Understanding-Windows-Message-Queues-for-the-Cshar
/// </remarks>
public sealed class HotKeyManager
{

    #region Fields

    private int _id = 0;

    #endregion

    #region Events

    public event EventHandler<HotKeyEventArgs>? HotKeyPressed;

    #endregion

    #region Constructors

    public HotKeyManager(Keystroke hotkey)
    {
        this.HotKey = hotkey ?? throw new ArgumentNullException(nameof(hotkey));
        // cache a window proc delegate so doesn't get garbage-collected
        this.WndProc = this.WindowProc;
        this.HWnd = IntPtr.Zero;
    }

    #endregion

    #region Properties

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public Keystroke HotKey
    {
        get;
    }

    private Winuser.WNDPROC WndProc
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

    #endregion

    #region Methods

    private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case Winuser.WM_HOTKEY:
                // https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
                var param = (uint)lParam.ToInt64();
                var key = (Keys)((param & 0xffff0000) >> 16);
                var modifiers = (KeyModifiers)(param & 0x0000ffff);
                var e = new HotKeyEventArgs(key, modifiers);
                this.OnHotKeyPressed(e);
                break;
                //case Winuser.WM_DESTROY:
                //    break;
                //case Winuser.WM_QUIT:
                //    break;
        }
        return Winuser.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void Start()
    {

        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/using-messages-and-message-queues

        var hInstance = Process.GetCurrentProcess().Handle;

        // see https://stackoverflow.com/a/30992796/3156906
        var wndClass = new Winuser.WNDCLASSEX
        {
            cbSize = Marshal.SizeOf(typeof(Winuser.WNDCLASSEX)),
            hInstance = hInstance,
            lpszClassName = "FancyMouseMessageClass",
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(this.WndProc),
        };

        var wndClassAtom = Win32Wrappers.RegisterClassEx(
            lpwcx: ref wndClass
        );

        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features#message-only-windows
        //     https://devblogs.microsoft.com/oldnewthing/20171218-00/?p=97595
        //     https://stackoverflow.com/a/30992796/3156906
        this.HWnd = Win32Wrappers.CreateWindowEx(
            dwExStyle: 0,
            lpClassName: wndClassAtom,
            lpWindowName: "FancyMouseMessageWindow",
            dwStyle: 0,
            x: 0,
            y: 0,
            nWidth: 300,
            nHeight: 400,
            hWndParent: Winuser.HWND_MESSAGE, // message-only window
            hMenu: IntPtr.Zero,
            hInstance: hInstance,
            lpParam: IntPtr.Zero
        );

        //var visible = Winuser.ShowWindow(
        //    hwnd, Winuser.SW_SHOW
        //);

        this.MessageLoop = new MessageLoop(
            name: "FancyMouseMessageLoop",
            isBackground: true
        );

        this.MessageLoop.Run();

        _ = Win32Wrappers.RegisterHotKey(
            hWnd: this.HWnd,
            id: Interlocked.Increment(ref _id),
            fsModifiers: (uint)this.HotKey.Modifiers,
            vk: (uint)this.HotKey.Key
        );

    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public void Stop()
    {
        _ = Win32Wrappers.UnregisterHotKey(
            hWnd: this.HWnd,
            id: this._id
        );
        this.MessageLoop?.Exit();
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    private void OnHotKeyPressed(HotKeyEventArgs e)
    {
        this.HotKeyPressed?.Invoke(null, e);
    }

    #endregion

}
