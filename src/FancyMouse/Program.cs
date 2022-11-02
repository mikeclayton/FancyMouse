using FancyMouse.Helpers;
using FancyMouse.UI;
using KeyboardWatcher.Extensions;
using System.Diagnostics;

namespace Kingsland.FancyMouse;

internal static class Program
{

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

        Application.EnableVisualStyles();

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // by default, Screen.Bounds returns the *scaled* resolution
        //
        // e.g.:
        //
        //   display resolution : 1920 x 1080
        //   scale              : 150%
        //
        // gives a screen with bounds:
        //
        //  width:  1280 (because 1280 * 150% = 1920)
        //  height: 720  (because  720 * 150% = 1080)
        //
        // so we need to tell the OS we understand DPI and it will then give us actual
        // display resolution (1920 x 1080) instead of the scaled resolution (1280 x 720).

        // Create a reference to the OS version of Windows 10 Creators Update.
        var osMinVersion = new Version(10, 0, 15063, 0);

        // check if we're running in a "high-dpi aware" process
        // (see https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8)
        var currentDpiAwareness = DpiAwarenessHelper.GetProcessDpiAwareness();
        var desiredDpiAwareness = NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
        if (currentDpiAwareness != desiredDpiAwareness)
        {
            // if not, can we *switch* to a "high-dpi aware" mode?
            // (see https://stackoverflow.com/a/28923832/3156906)
            DpiAwarenessHelper.SetProcessDpiAwareness(desiredDpiAwareness);
        }


        Console.WriteLine("Hello, World!");

        // create the notify icon for the application
        var notifyForm = new FancyMouseNotify();

        var dialog = new FancyMouseDialog(
            new FancyMouseDialogOptions(
                maximumSize: new Size(1600, 1200)
            )
        );

        var keyboardWatcher = new KeyboardWatcher.KeyboardWatcher();
        keyboardWatcher.KeyUp += (s, e) => {
            if (e.KeyData == (Keys.Control | Keys.Alt | Keys.Shift | Keys.F))
            {
                Console.WriteLine(e.ToKeySequence());
                dialog.Show();
            }
        };
        keyboardWatcher.Start();

        Application.Run();

        //// message loop
        //while (true)
        //{
        //    if (Winuser.PeekMessage(out var msg, IntPtr.Zero, 0, 0, Winuser.PM_REMOVE))
        //    {
        //        if (msg.message == Winuser.WM_QUIT)
        //        {
        //            break;
        //        }
        //        Winuser.TranslateMessage(ref msg);
        //        Winuser.DispatchMessage(ref msg);
        //    }
        //}
        
    }

}