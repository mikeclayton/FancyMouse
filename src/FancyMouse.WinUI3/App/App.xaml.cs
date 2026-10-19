using System.Globalization;

using FancyMouse.Telemetry;
using FancyMouse.Windows.Helpers;
using FancyMouse.WinUI3.Internal.Helpers;
using FancyMouse.WinUI3.UI;

using Microsoft.UI.Xaml;
using NLog;
using NLog.Targets;

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
    public App(TelemetryContext telemetry)
    {
        this.Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));

        this.InitializeComponent();
        this.ActivationTask = Task.CompletedTask;
    }

    private TelemetryContext Telemetry
    {
        get;
    }

    private TrayIcon? TrayIcon
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the <see cref="CancellationTokenSource"/> for the
    /// currently running hotkey activation if there is one, otherwise
    /// returns null.
    /// </summary>
    private CancellationTokenSource? ActivationCancellation
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets most recently started hotkey activation.
    /// </summary>
    private Task ActivationTask
    {
        get;
        set;
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // shared by InitializeLogger and InitializeTelemetry, so a launch's log file and
        // telemetry file can be matched up by filename alone.
        var timestamp = DateTime.Now;
        this.InitializeLogger(timestamp);

        var logger = LogManager.GetCurrentClassLogger();
        logger.Info("app launched");

        try
        {
            var configHelper = new ConfigHelper(logger);
            configHelper.SetAppSettingsPath(App.GetAppSettingsPath(logger));

            var previewWindow = new PreviewWindow(logger: logger, telemetry: this.Telemetry, configHelper: configHelper);

            this.VerifyHighDpiMode(logger);
            this.InitializeAppSettings(logger, configHelper);
            this.InitializeTelemetry(timestamp, configHelper);
            this.InitializeHotkey(logger, previewWindow, configHelper);
            this.InitializeTrayIcon(logger);
        }
        catch (Exception ex)
        {
            logger.Error(ex);
            LogManager.Flush();
            throw;
        }
    }
}
