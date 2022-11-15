using FancyMouse.Helpers;
using FancyMouse.HotKeyManager;
using FancyMouse.UI;
using System.Diagnostics;

namespace FancyMouse;

internal static class Program
{

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {


        // replaced keyboardwatcher with hotkeymanager

        // run Logitech SetPoint as admin for Office to work with custom mouse bindings.
        // (keyboard bindings work fine as normal user in Office, just mouse bindings don't...)
        // https://social.msdn.microsoft.com/Forums/en-US/09a7ebee-9567-4704-be88-de54a16ca99e/logitech-mouse-button-assignments-ignored-by-vs?forum=csharpide

        // scheduled task to start app at logon

        Application.EnableVisualStyles();

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        Program.ConfigureProcessDpiAwareness();

        Console.WriteLine("Hello, World!");

        // create the notify icon for the application
        var notifyForm = new FancyMouseNotify();

        var dialog = new FancyMouseDialog(
            new FancyMouseDialogOptions(
                maximumSize: new Size(1600, 1200)
            )
        );

        var keys = HotKeyManager.HotKeyManager.Parse(
            "CTRL + ALT + SHIFT + F"
        );

        var hotKeyManager = new HotKeyManager.HotKeyManager();
        hotKeyManager.RegisterHotKey(keys.Keys, keys.Modifiers);
        hotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(
            (s, e) => {
                dialog.Show();
            }
        );

        Application.Run();

        // message loop
        //MessageLoop.Run();

        //keyboardWatcher.Stop();

    }

    private static void ConfigureProcessDpiAwareness()
    {

        // by default, Screen.Bounds returns the *scaled* resolution of the desktop adjusted
        // for high DPI awareness - e.g.:
        //
        //   display resolution : 1920 x 1080
        //   scale              : 150%
        //
        // gives a screen with bounds:
        //
        //   width:  1280 (because 1280 * 150% = 1920)
        //   height: 720  (because  720 * 150% = 1080)
        //
        // *but* this means when we work out the size of our desktop screenshot we don't get
        // the actual pixel dimensions - we get the adjusted value which distorts where we
        // move the mouse to when the preview thumbnail gets clicked.
        //
        // *so* we need to tell the OS we understand DPI and it will then give us actual
        // display resolution (1920 x 1080) instead of the scaled resolution (1280 x 720)

        // Windows 10 Creators Update
        var osMinVersion = new Version(10, 0, 15063, 0);

        // get the current process's dpi awareness mode
        // see https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8
        var process = Process.GetCurrentProcess();
        var apiResult = NativeMethods.GetProcessDpiAwareness(
            process.Handle,
            out var currentDpiAwareness
        );
        if (apiResult != NativeMethods.S_OK)
        {
            throw new InvalidOperationException(
                $"{nameof(NativeMethods.GetProcessDpiAwareness)} returned {apiResult}"
            );
        }

        var desiredDpiAwareness = NativeMethods.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
        if (currentDpiAwareness != desiredDpiAwareness)
        {
            // try to set the current process's dpi awarenees mode.
            // see https://stackoverflow.com/a/28923832/3156906
            //     https://learn.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8
            apiResult = NativeMethods.SetProcessDpiAwareness(desiredDpiAwareness);
            switch (apiResult)
            {
                case NativeMethods.S_OK:
                    break;
                default:
                    throw new InvalidOperationException(
                        $"{nameof(NativeMethods.SetProcessDpiAwareness)} returned {apiResult}"
                    );
            }
        }

    }

}