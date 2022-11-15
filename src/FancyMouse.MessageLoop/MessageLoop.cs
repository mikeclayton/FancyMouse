using FancyMouse.MessageLoop.Win32Api;

namespace FancyMouse.MessageLoop;

public static class MessageLoop
{

    public static void Run()
    {
        while (true)
        {
            if (Winuser.PeekMessage(out var msg, IntPtr.Zero, 0, 0, Winuser.PM_REMOVE))
            {
                if (msg.message == Winuser.WM_QUIT)
                {
                    break;
                }
                Winuser.TranslateMessage(ref msg);
                Winuser.DispatchMessage(ref msg);
            }
        }
    }

}
