using FancyMouse.WindowsHotKeys.Win32Api;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;

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

    delegate bool RegisterHotKeyDelegate(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    delegate bool UnregisterHotKeyDelegate(IntPtr hWnd, int id);

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

    public Keystroke HotKey
    {
        get;
    }

    private Winuser.WNDPROC WndProc
    {
        get;
        set;
    }

    private IntPtr HWnd
    {
        get;
        set;
    }

    #endregion

    #region Win32Api Methods

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
            //    DestroyWindow(hWnd);
            //    //If you want to shutdown the application, call the next function instead of DestroyWindow
            //    PostQuitMessage(0);
            //    break;
            default:
                break;
        }
        return Winuser.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static ushort RegisterClass(IntPtr hInstance, string className, Winuser.WNDPROC wndProc)
    {
        // see https://stackoverflow.com/a/30992796/3156906
        var wndClass = new Winuser.WNDCLASSEX
        {
            cbSize = Marshal.SizeOf(typeof(Winuser.WNDCLASSEX)),
            hInstance = hInstance,
            lpszClassName = className,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
        };
        var wndClassAtom = Winuser.RegisterClassEx(ref wndClass);
        if (wndClassAtom == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException
                ($"{nameof(Winuser.RegisterClassEx)} failed with result {wndClassAtom}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error)
            );
        }
        return wndClassAtom;
    }

    private static IntPtr CreateWindow(IntPtr hInstance, ushort wndClassAtom, string windowName)
    {
        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features#message-only-windows
        //     https://devblogs.microsoft.com/oldnewthing/20171218-00/?p=97595
        //     https://stackoverflow.com/a/30992796/3156906
        var hwnd = Winuser.CreateWindowEx(
            dwExStyle: 0,
            lpClassName: wndClassAtom,
            lpWindowName: windowName,
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
        if (hwnd == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException
                ($"{nameof(Winuser.CreateWindowEx)} failed with result {hwnd}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error)
            );
        }
        //var visible = Winuser.ShowWindow(
        //    hwnd, Winuser.SW_SHOW
        //);
        return hwnd;
    }

    private int RegisterHotKey()
    {
        var id = Interlocked.Increment(ref _id);
        var result = Winuser.RegisterHotKey(
            this.HWnd,
            id,
            (uint)this.HotKey.Modifiers,
            (uint)this.HotKey.Key
        );
        if (result == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException
                ($"{nameof(Winuser.RegisterHotKey)} failed with result {result}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error)
            );
        }
        return id;
    }

    public void UnregisterHotKey(int id)
    {
        var result = Winuser.UnregisterHotKey(this.HWnd, id);
        if (result == 0)
        {
            var lastWin32Error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException
                ($"{nameof(Winuser.UnregisterHotKey)} failed with result {result}. GetLastWin32Error returned '{lastWin32Error}'.",
                new Win32Exception(lastWin32Error)
            );
        }
    }

    #endregion

    #region Methods

    public void Start()
    {

        // see https://learn.microsoft.com/en-us/windows/win32/winmsg/using-messages-and-message-queues

        var hInstance = Process.GetCurrentProcess().Handle;

        var wndClassAtom = HotKeyManager.RegisterClass(
            hInstance: hInstance,
            className: "FancyMouseMessageClass",
            wndProc: this.WndProc
        );

        this.HWnd = HotKeyManager.CreateWindow(
            hInstance: hInstance,
            wndClassAtom: wndClassAtom,
            windowName: "FancyMouseMessageWindow"
        );

        var messageLoop = new Thread(delegate ()
        {
            MessageLoop.Run(this.HWnd);
        })
        {
            Name = "FancyMouseMessageLoop",
            IsBackground = true
        };

        messageLoop.Start();

        this.RegisterHotKey();

    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    internal void OnHotKeyPressed(HotKeyEventArgs e)
    {
        this.HotKeyPressed?.Invoke(null, e);
    }

    #endregion

}
