using FancyMouse.WindowsHotKeys.Interop;

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

    private int? NativeThreadId
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
        if (this.ManagedThread != null)
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
        User32.MSG msg;

        var quitMessagePosted = false;

        // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessage
        //     https://learn.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues
        //     https://devblogs.microsoft.com/oldnewthing/20050405-46/?p=35973
        //     https://devblogs.microsoft.com/oldnewthing/20050406-57/?p=35963
        while (true)
        {
            _ = User32.GetMessageW(
                lpMsg: out msg,
                hWnd: IntPtr.Zero,
                wMsgFilterMin: 0,
                wMsgFilterMax: 0);

            if (msg.message == User32.WindowMessages.WM_QUIT)
            {
                break;
            }

            _ = User32.TranslateMessage(ref msg);
            _ = User32.DispatchMessage(ref msg);

            if ((this.CancellationTokenSource?.IsCancellationRequested ?? false) && !quitMessagePosted)
            {
                User32.PostQuitMessage(0);
                quitMessagePosted = true;
            }
        }

        // clean up
        this.ManagedThread = null;
        this.NativeThreadId = null;
        (this.CancellationTokenSource ?? throw new InvalidOperationException())
            .Dispose();
    }

    public void Exit()
    {
        if (this.ManagedThread == null)
        {
            throw new InvalidOperationException();
        }

        (this.CancellationTokenSource ?? throw new InvalidOperationException())
            .Cancel();

        // post a null message just to nudge the message loop and pump it - it'll notice that we've
        // set the cancellation token, then post a quit message to itself and exit the loop
        // see https://devblogs.microsoft.com/oldnewthing/20050405-46/?p=35973
        _ = Win32Wrappers.PostThreadMessageW(
            idThread: this.NativeThreadId ?? throw new InvalidOperationException(),
            Msg: User32.WindowMessages.WM_NULL,
            wParam: IntPtr.Zero,
            lParam: IntPtr.Zero);
    }
}
