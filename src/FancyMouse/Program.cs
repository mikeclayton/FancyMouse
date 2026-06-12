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
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // make sure we're in the right high dpi mode otherwise pixel positions and sizes for
        // screen captures get distorted and various coordinates aren't calculated correctly.
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
        // application thread otherwise we might block something important like the
        // hotkey message loop
        _ = previewForm.Handle;

        var cancellationTokenSource = new CancellationTokenSource();
        ConfigHelper.SetAppSettingsPath(".\\appSettings.json");
        ConfigHelper.SetHotKeyEventHandler(
            async (_, _) =>
            {
                // invoke on the thread the form was created on. this avoids
                // blocking the calling thread (e.g. the message loop as a
                // result of hotkey activation)
                await previewForm.InvokeAsync(
                    async (cancellationToken) =>
                    {
                        await previewForm.ShowPreviewAsync();
                    },
                    cancellationTokenSource.Token);
            });

        // load the application settings and start the filesystem watcher
        // so we reload if it changes
        ConfigHelper.LoadAppSettings();
        ConfigHelper.StartAppSettingsWatcher();

        Application.Run();
    }
}
