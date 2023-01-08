using FancyMouse.WindowsHotKeys.Win32Api;

namespace FancyMouse.WindowsHotKeys.Internal;

internal sealed class MessageLoop
{

    #region Constructors

    public MessageLoop(string name, bool isBackground)
    {
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
        this.IsBackground = isBackground;
    }

    #endregion

    #region Properties

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

    #endregion

    #region Methods

    public void Run()
    {
        if (this.ManagedThread != null)
        {
            throw new InvalidOperationException();
        }
        this.CancellationTokenSource = new CancellationTokenSource();
        this.ManagedThread = new Thread(delegate ()
        {
            this.NativeThreadId = Kernel32.GetCurrentThreadId();
            this.RunInternal();
        })
        {
            Name = this.Name,
            IsBackground = this.IsBackground
        };
        this.ManagedThread.Start();
    }

    private void RunInternal()
    {

        var msg = default(Winuser.MSG);

        var quitMessagePosted = false;

        // see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessage
        //     https://learn.microsoft.com/en-us/windows/win32/winmsg/about-messages-and-message-queues
        //     https://devblogs.microsoft.com/oldnewthing/20050405-46/?p=35973
        //     https://devblogs.microsoft.com/oldnewthing/20050406-57/?p=35963
        while (true)
        {

            _ = Winuser.GetMessage(
                lpMsg: out msg,
                hWnd: IntPtr.Zero,
                wMsgFilterMin: 0,
                wMsgFilterMax: 0
            );

            if (msg.message == Winuser.WM_QUIT)
            {
                break;
            }

            _ = Winuser.TranslateMessage(ref msg);
            _ = Winuser.DispatchMessage(ref msg);

            if ((this.CancellationTokenSource?.IsCancellationRequested ?? false) && !quitMessagePosted)
            {
                Winuser.PostQuitMessage(0);
                quitMessagePosted = true;
            }

        }

        // clean up
        this.ManagedThread = null;
        this.NativeThreadId = null;
        (this.CancellationTokenSource ?? throw new NullReferenceException())
            .Dispose();

    }

    public void Exit()
    {

        if (this.ManagedThread == null)
        {
            throw new InvalidOperationException();
        }

        (this.CancellationTokenSource ?? throw new NullReferenceException())
            .Cancel();

        // post a null message just to nudge the message loop and pump it - it'll notice that we've
        // set the cancellation token, then post a quit message to itself and exit the loop
        // see https://devblogs.microsoft.com/oldnewthing/20050405-46/?p=35973
        _ = Win32Wrappers.PostThreadMessage(
            idThread: (this.NativeThreadId ?? throw new NullReferenceException()),
            Msg: Winuser.WM_NULL,
            wParam: IntPtr.Zero,
            lParam: IntPtr.Zero
        );

    }

    #endregion

}
