using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;
using static FancyMouse.NativeMethods.User32;

namespace FancyMouse.WindowsHotKeys.Internal;

internal sealed class MessageLoop
{
    public MessageLoop(string name, bool isBackground)
    {
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
        this.IsBackground = isBackground;
    }

    private string Name
    {
        get;
    }

    private bool IsBackground
    {
        get;
    }

    private Thread? ManagedThread
    {
        get;
        set;
    }

    private DWORD NativeThreadId
    {
        get;
        set;
    }

    private CancellationTokenSource? CancellationTokenSource
    {
        get;
        set;
    }

    public void Run()
    {
        if (this.ManagedThread is not null)
        {
            throw new InvalidOperationException();
        }

        this.CancellationTokenSource = new CancellationTokenSource();
        this.ManagedThread = new Thread(() =>
        {
            this.NativeThreadId = Kernel32.GetCurrentThreadId();
            this.RunInternal();
        })
        {
            Name = this.Name,
            IsBackground = this.IsBackground,
        };

        this.ManagedThread.Start();
    }

    private void RunInternal()
    {
        var quitMessagePosted = false;

        var lpMsg = new LPMSG(
            new MSG(
                hwnd: HWND.Null,
                message: MESSAGE_TYPE.WM_QUIT,
                wParam: new(0),
                lParam: new(0),
                time: new(0),
                pt: new(0, 0),
                lPrivate: new(0)));

        // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessage
        //     https://learn.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues
        //     https://devblogs.microsoft.com/oldnewthing/20050405-46/?p=35973
        //     https://devblogs.microsoft.com/oldnewthing/20050406-57/?p=35963
        while (true)
        {
            var result = User32.GetMessageW(
                lpMsg: lpMsg,
                hWnd: HWND.Null,
                wMsgFilterMin: 0,
                wMsgFilterMax: 0);
            if (result.Value == -1)
            {
                continue;
            }

            var msg = lpMsg.ToStructure();

            if (msg.message == MESSAGE_TYPE.WM_QUIT)
            {
                break;
            }

            _ = User32.TranslateMessage(msg);
            _ = User32.DispatchMessageW(msg);

            if ((this.CancellationTokenSource?.IsCancellationRequested ?? false) && !quitMessagePosted)
            {
                User32.PostQuitMessage(0);
                quitMessagePosted = true;
            }
        }

        // clean up
        this.ManagedThread = null;
        this.NativeThreadId = 0;
        (this.CancellationTokenSource ?? throw new InvalidOperationException())
            .Dispose();
    }

    public void Exit()
    {
        if (this.ManagedThread is null)
        {
            throw new InvalidOperationException();
        }

        (this.CancellationTokenSource ?? throw new InvalidOperationException())
            .Cancel();

        // post a null message just to nudge the message loop and pump it - it'll notice that we've
        // set the cancellation token, then post a quit message to itself and exit the loop
        // see https://devblogs.microsoft.com/oldnewthing/20050405-46/?p=35973
        _ = Win32Wrappers.PostThreadMessageW(
            idThread: this.NativeThreadId,
            Msg: MESSAGE_TYPE.WM_NULL,
            wParam: WPARAM.Null,
            lParam: LPARAM.Null);
    }
}
