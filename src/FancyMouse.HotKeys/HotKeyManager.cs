using FancyMouse.Common.Helpers;
using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.HotKeys;

/// <remarks>
/// See https://stackoverflow.com/a/3654821/3156906
///     https://learn.microsoft.com/en-us/archive/msdn-magazine/2007/june/net-matters-handling-messages-in-console-apps
///     https://www.codeproject.com/Articles/5274425/Understanding-Windows-Message-Queues-for-the-Cshar
/// </remarks>
public sealed class HotKeyManager
{
    public event EventHandler<HotKeyEventArgs>? HotKeyPressed;

    public HotKeyManager()
    {
        // cache the window proc delegate so it doesn't get garbage-collected
        this.WndProc = this.WindowProc;

        this.MessageSemaphore = new(0, 1);
        this.MessageLoop = new MessageLoop(
            name: "FancyMouseMessageLoop",
            hwndCallback: () =>
            {
                if (this.Hwnd.IsNull)
                {
                    (this.WndClass, this.Hwnd) = Win32Helper.User32.CreateMessageOnlyWindow(
                        "FancyMouseMessageClass", "FancyMouseMessageWindow", this.WndProc);
                }

                return this.Hwnd;
            });
        this.MessageLoop.Start();
    }

    private WNDPROC WndProc
    {
        get;
    }

    private ATOM? WndClass
    {
        get;
        set;
    }

    private HWND Hwnd
    {
        get;
        set;
    }

    private MessageLoop MessageLoop
    {
        get;
    }

    public Keystroke? HotKey
    {
        get;
        private set;
    }

    private SemaphoreSlim MessageSemaphore
    {
        get;
    }

    public void SetHoKey(Keystroke? hotKey)
    {
        var hwnd = this.MessageLoop.Hwnd;

        // do we need to unregister the existing hotkey first?
        if ((this.HotKey is not null) && hwnd.HasValue)
        {
            Win32Helper.User32.PostMessage(
                hwnd.Value, HotKeyHelper.WM_PRIV_UNREGISTER_HOTKEY, WPARAM.Null, LPARAM.Null);
            this.MessageSemaphore.Wait();
        }

        this.HotKey = hotKey;

        // register the new hotkey
        if ((this.HotKey is not null) && hwnd.HasValue)
        {
            Win32Helper.User32.PostMessage(hwnd.Value, HotKeyHelper.WM_PRIV_REGISTER_HOTKEY, WPARAM.Null, LPARAM.Null);
            this.MessageSemaphore.Wait();
        }
    }

    private LRESULT WindowProc(HWND hWnd, MESSAGE_TYPE msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case MESSAGE_TYPE.WM_HOTKEY:
            {
                // https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
                // https://stackoverflow.com/a/47831305/3156906
                var param = (uint)lParam.Value.ToInt64();
                var key = (Keys)((param & 0xffff0000) >> 16);
                var modifiers = (KeyModifiers)(param & 0x0000ffff);
                var e = new HotKeyEventArgs(key, modifiers);
                this.OnHotKeyPressed(e);
                break;
            }

            case HotKeyHelper.WM_PRIV_REGISTER_HOTKEY:
            {
                Win32Helper.User32.RegisterHotKey(
                    hWnd: this.MessageLoop.Hwnd ?? throw new InvalidOperationException(),
                    id: 1,
                    fsModifiers: (HOT_KEY_MODIFIERS)(this.HotKey ?? throw new InvalidOperationException()).Modifiers,
                    vk: (uint)this.HotKey.Key);
                this.MessageSemaphore.Release();
                break;
            }

            case HotKeyHelper.WM_PRIV_UNREGISTER_HOTKEY:
            {
                Win32Helper.User32.UnregisterHotKey(
                    hWnd: this.MessageLoop.Hwnd ?? throw new InvalidOperationException(),
                    id: 1);
                this.MessageSemaphore.Release();
                break;
            }
        }

        var result = Win32Helper.User32.DefWindowProc(hWnd, msg, wParam, lParam);
        return result;
    }

    private void OnHotKeyPressed(HotKeyEventArgs e)
    {
        this.HotKeyPressed?.Invoke(null, e);
    }
}
