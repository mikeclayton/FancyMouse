using System.Globalization;

using FancyMouse.Common.Helpers;
using FancyMouse.WinUI3.Internal.Helpers;
using FancyMouse.WinUI3.UI;
using Microsoft.UI.Xaml;
using NLog;

using Application = Microsoft.UI.Xaml.Application;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace FancyMouse.WinUI3;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    private TrayIcon? TrayIcon
    {
        get;
        set;
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var logger = LogManager.GetCurrentClassLogger();
        logger.Info("app launched");

        try
        {
            var previewWindow = new PreviewWindow(logger: logger);

            // run Logitech SetPoint as admin for hotkeys to get activated from custom mouse bindings
            // when an Office application or Visual Studio is the active window. (SetPoint *keyboard*
            // bindings work fine when running as a normal user in Office, but *mouse* bindings only
            // work when SetPoint is run as an admin...)
            // https://social.msdn.microsoft.com/Forums/en-US/09a7ebee-9567-4704-be88-de54a16ca99e/logitech-mouse-button-assignments-ignored-by-vs?forum=csharpide

            // make sure we're in the right high dpi mode otherwise pixel positions and sizes for
            // screen captures get distorted and various coordinates aren't calculated correctly.
            logger.Info("checking high dpi mode");
            DpiModeHelper.EnsurePerMonitorV2Enabled();
            logger.Info("high dpi mode is ok");

            var appSettingsPath = ".\\appSettings.json";
            logger.Info(CultureInfo.InvariantCulture, "settings path = {appSettingsPath}", appSettingsPath);
            ConfigHelper.SetAppSettingsPath(appSettingsPath);

            // load the application settings and start the filesystem watcher
            // so we reload if it changes
            logger.Info("loading app settings");
            ConfigHelper.LoadAppSettings();
            ConfigHelper.StartAppSettingsWatcher();
            logger.Info("loaded app settings");

            logger.Info("starting hotkey handler");
            ConfigHelper.SetHotKeyEventHandler(
                (_, _) =>
                {
                    // invoke on the thread the form was created on. this avoids
                    // blocking the calling thread (e.g. the message loop as a
                    // result of hotkey activation)
                    previewWindow.DispatcherQueue.TryEnqueue(
                        async () =>
                        {
                            await previewWindow.ShowPreview();
                        });
                });
            logger.Info("started hotkey handler");

            // create the system tray icon
            var trayIcon = new TrayIcon();
            trayIcon.ExitCommandClicked += (sender, e) =>
            {
                App.Current.Exit();
            };
            trayIcon.Initialize();
            this.TrayIcon = trayIcon;
        }
        catch (Exception ex)
        {
            logger.Error(ex);
            LogManager.Flush();
            throw;
        }
    }
}
