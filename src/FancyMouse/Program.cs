using FancyMouse.Internal.Helpers;
using FancyMouse.UI;
using NLog;

namespace FancyMouse;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // run Logitech SetPoint as admin for hotkeys to get activated from custom mouse bindings
        // when an Office application or Visual Studio is the active window. (SetPoint *keyboard*
        // bindings work fine when running as a normal user in Office, but *mouse* bindings only
        // work when SetPoint is run as an admin...)
        // https://social.msdn.microsoft.com/Forums/en-US/09a7ebee-9567-4704-be88-de54a16ca99e/logitech-mouse-button-assignments-ignored-by-vs?forum=csharpide

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // make sure we're in the right high dpi mode otherwise pixel positions and sizes for
        // screen captures get distorted and coordinates aren't various calculated correctly.
        if (Application.HighDpiMode != HighDpiMode.PerMonitorV2)
        {
            throw new InvalidOperationException("high dpi mode is not set to PerMonitorV2");
        }

        // create the notify icon for the application
        var notifyForm = new FancyMouseNotify();

        var previewForm = new FancyMouseForm(
            logger: LogManager.CreateNullLogger());

        // touch the form handle - this will force the handle to be created if it hasn't
        // been already (we'll get an error from previewForm.BeginInvoke() if the form
        // handle doesn't exist). note that BeginInvoke() will block whatever thread is
        // the owner of the handle so we need to make sure it gets created on the main
        // application thread otherwise we might block something important like the the
        // hotkey message loop
        var previewHwnd = previewForm.Handle;

        ConfigHelper.SetAppSettingsPath(".\\appSettings.json");
        ConfigHelper.SetHotKeyEventHandler(
            (_, _) =>
            {
                // invoke on the thread the form was created on. this avoids
                // blocking the calling thread (e.g. the message loop as a
                // result of hotkey activation)
                previewForm.BeginInvoke(
                    () =>
                    {
                        previewForm.ShowPreview();
                    });
            });

        // load the application settings and start the filesystem watcher
        // so we reload if it changes
        ConfigHelper.LoadAppSettings();
        ConfigHelper.StartAppSettingsWatcher();

        Application.Run();
    }
}
