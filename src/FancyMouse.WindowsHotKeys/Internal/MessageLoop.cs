using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;
using static FancyMouse.NativeMethods.User32;

namespace FancyMouse.WindowsHotKeys.Internal;

internal sealed class MessageLoop
{
    public MessageLoop(string name)
    {
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
        this.RunningSemaphore = new SemaphoreSlim(1);
        this.CancellationTokenSource = new CancellationTokenSource();
    }

    private string Name
    {
        get;
    }

    /// <summary>
    /// Gets a semaphore that can be waited on until the message loop has stopped.
    /// </summary>
    private SemaphoreSlim RunningSemaphore
    {
        get;
    }

    /// <summary>
    /// Gets a cancellation token that can be used to signal the internal message loop thread to stop.
    /// </summary>
    private CancellationTokenSource CancellationTokenSource
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

    public void Start()
    {
        // make sure we're not already running the internal message loop
        if (!this.RunningSemaphore.Wait(0))
        {
            throw new InvalidOperationException();
        }

        // reset the internal message loop cancellation token
        if (!this.CancellationTokenSource.TryReset())
        {
            throw new InvalidOperationException();
        }

        // start a new internal message loop thread
        this.ManagedThread = new Thread(() =>
        {
            this.NativeThreadId = Kernel32.GetCurrentThreadId();
            this.RunMessageLoop();
        })
        {
            Name = this.Name,
            IsBackground = true,
        };
        this.ManagedThread.Start();
    }

    private void RunMessageLoop()
    {
        var lpMsg = new LPMSG(
            new MSG(
                hwnd: HWND.Null,
                message: MESSAGE_TYPE.WM_NULL,
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
            // check if the cancellation token is signalling that we should stop the message loop
            if (this.CancellationTokenSource.IsCancellationRequested)
            {
                break;
            }

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
        }

        // clean up
        this.ManagedThread = null;
        this.NativeThreadId = 0;

        // the message loop is no longer running
        this.RunningSemaphore.Release(1);
    }

    public void Stop()
    {
        // make sure we're actually running the internal message loop
        if (this.RunningSemaphore.CurrentCount != 0)
        {
            throw new InvalidOperationException();
        }

        // signal to the internal message loop that it should stop
        (this.CancellationTokenSource ?? throw new InvalidOperationException())
            .Cancel();

        // post a null message just in case GetMessageW needs a nudge to stop blocking the
        // message loop. the loop will then notice that we've set the cancellation token,
        // and exit the loop...
        // (see https://devblogs.microsoft.com/oldnewthing/20050405-46/?p=35973)
        _ = Win32Wrappers.PostThreadMessageW(
            idThread: this.NativeThreadId,
            Msg: MESSAGE_TYPE.WM_NULL,
            wParam: WPARAM.Null,
            lParam: LPARAM.Null);

        // wait for the internal message loop to actually stop
        this.RunningSemaphore.Wait();
    }
}
