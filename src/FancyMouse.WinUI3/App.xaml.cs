using System.Globalization;

using FancyMouse.Common.Helpers;
using FancyMouse.Common.Telemetry;
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
    public App()
    {
        this.InitializeComponent();
        this.ActivationTask = Task.CompletedTask;
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
            var previewWindow = new PreviewWindow(logger: logger);

            this.VerifyHighDpiMode(logger);
            this.InitializeAppSettings(logger);
            this.InitializeTelemetry(timestamp);
            this.InitializeHotkey(logger, previewWindow);
            this.InitializeTrayIcon();
        }
        catch (Exception ex)
        {
            logger.Error(ex);
            LogManager.Flush();
            throw;
        }
    }

    /// <summary>
    /// Points NLog's "logfile" target (see NLog.config) at a file stamped with
    /// <paramref name="timestamp"/>, in place of the fixed layout NLog.config would otherwise
    /// use - so the log file's name can share <paramref name="timestamp"/> with
    /// <see cref="InitializeTelemetry"/>'s telemetry file, making the two easy to match up for a
    /// given launch. Must run before anything logs, so the very first message lands in the
    /// right file.
    /// </summary>
    private void InitializeLogger(DateTime timestamp)
    {
        if (LogManager.Configuration?.FindTargetByName<FileTarget>("logfile") is { } fileTarget)
        {
            fileTarget.FileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FancyMouse",
                "Logs",
                $"applog_{timestamp:yyyy-MM-dd_HH_mm_ss}.txt");
            LogManager.ReconfigExistingLoggers();
        }
    }

    /// <summary>
    /// Makes sure we're in the right high dpi mode - otherwise pixel positions
    /// and sizes for screen captures get distorted and various coordinates aren't
    /// calculated correctly.
    /// </summary>
    private void VerifyHighDpiMode(Logger logger)
    {
        logger.Info("checking high dpi mode");
        DpiModeHelper.EnsurePerMonitorV2Enabled();
        logger.Info("high dpi mode is ok");
    }

    /// <summary>
    /// Points <see cref="ConfigHelper"/> at the config file, loads it,
    /// and starts the filesystem watcher that reloads it whenever it changes.
    /// Must run before <see cref="InitializeTelemetry"/>, which depends on
    /// <see cref="ConfigHelper.AppSettings"/> being populated already.
    /// </summary>
    private void InitializeAppSettings(Logger logger)
    {
        var appSettingsPath = ".\\appSettings.json";
        logger.Info(CultureInfo.InvariantCulture, "settings path = {appSettingsPath}", appSettingsPath);
        ConfigHelper.SetAppSettingsPath(appSettingsPath);

        // load the application settings and start the filesystem watcher
        // so we can automatically reload settings if they change on disk
        logger.Info("loading app settings");
        ConfigHelper.LoadAppSettings();
        ConfigHelper.StartAppSettingsWatcher();
        logger.Info("loaded app settings");
    }

    /// <summary>
    /// Sets the process-wide ambient <see cref="Telemetry.Current"/> and, if
    /// <see cref="Settings.AppSettings.TelemetryEnabled"/> says so, starts it
    /// writing to a file stamped with <paramref name="timestamp"/> - see
    /// <see cref="InitializeLogger"/> for why the same timestamp is shared with the log file.
    /// Must run after <see cref="ConfigHelper.LoadAppSettings"/>, since it depends on
    /// <see cref="ConfigHelper.AppSettings"/> being populated already.
    /// </summary>
    private void InitializeTelemetry(DateTime timestamp)
    {
        Telemetry.SetCurrent(TelemetryContext.Create(nameof(FancyMouse)));

        if (ConfigHelper.AppSettings?.TelemetryEnabled == true)
        {
            var telemetryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FancyMouse",
                "Telemetry",
                $"telemetry_{timestamp:yyyy-MM-dd_HH_mm_ss}.jsonl");
            Telemetry.Current.Start(new FileTelemetryWriter(telemetryPath));
        }
    }

    /// <summary>
    /// Wires the configured hotkey to show <paramref name="previewWindow"/>'s preview - owns
    /// making sure only one <see cref="PreviewWindow.ShowPreviewAsync"/> call is ever running at
    /// a time via <see cref="ActivationCancellation"/>/<see cref="ActivationTask"/> -
    /// <see cref="PreviewWindow"/> itself is deliberately unaware of this; it just accepts a
    /// <see cref="CancellationToken"/> and stops promptly when told to (see
    /// <see cref="PreviewWindow.ShowPreviewAsync"/>'s remarks). Rapid repeat activation (e.g.
    /// spam-pressing the hotkey) would otherwise let two calls interleave and mutate the
    /// window's UI state at the same time, which is visible as a brief white flash before the
    /// correct content redraws.
    /// </summary>
    private void InitializeHotkey(Logger logger, PreviewWindow previewWindow)
    {
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
                        // supersede whatever activation is currently running (if any), and
                        // wait for it to actually stop before this one starts - not just
                        // signal cancellation and hope
                        this.ActivationCancellation?.Cancel();
                        try
                        {
                            await this.ActivationTask.ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            // expected - the previous activation observed the cancellation
                            // above and stopped
                        }

                        this.ActivationCancellation?.Dispose();
                        var cancellation = new CancellationTokenSource();
                        this.ActivationCancellation = cancellation;

                        var task = previewWindow.ShowPreviewAsync(cancellation.Token);
                        this.ActivationTask = task;
                        try
                        {
                            await task.ConfigureAwait(false);

                            // each activation churns through several WriteableBitmap
                            // pixel buffers (background, bezels, content) - left to the GC's
                            // own schedule, those pile up as uncollected garbage quickly
                            // enough under repeat activation to show up as inflated memory
                            // usage in Task Manager. Collecting here at the end of an
                            // activation with a background (non-blocking) collection
                            // frees up the unused memory and avoids stalling this thread
                            // the way a blocking GC.Collect() and WaitForPendingFinalizers()
                            // would.
                            GC.Collect(2, GCCollectionMode.Optimized, blocking: false);
                        }
                        catch (OperationCanceledException)
                        {
                            // expected - this activation was cancelled due to a newer
                            // activation being triggered before this one finished
                        }
                        catch (Exception ex)
                        {
                            // this callback runs as a fire-and-forget async lambda passed
                            // to DispatcherQueue.TryEnqueue - anything thrown here gets swallowed
                            // by the WinAppSDK's own dispatcher loop, so we just log it for
                            // visibility and then swallow it ourselves
                            logger.Error(ex, "unhandled exception while showing the preview window");
                        }
                    });
            });
        logger.Info("started hotkey handler");
    }

    /// <summary>
    /// Creates the system tray icon and wires its exit command to close the application.
    /// </summary>
    private void InitializeTrayIcon()
    {
        var trayIcon = new TrayIcon();
        trayIcon.ExitCommandClicked += (sender, e) =>
        {
            // FileTelemetryWriter only flushes its buffered lines on Stop()/Dispose() (a
            // deliberate trade-off against flushing on every single record) - nothing else in
            // the app's lifetime calls that, so without this, telemetry from the whole session
            // never reaches disk at all once the process exits.
            Telemetry.Current.Stop();
            App.Current.Exit();
        };
        this.TrayIcon = trayIcon;
    }
}
