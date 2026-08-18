using System.Globalization;

using FancyMouse.Common.Helpers;
using FancyMouse.Common.Telemetry;
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

        Telemetry.SetCurrent(TelemetryContext.Create(nameof(FancyMouse)));

        try
        {
            var previewWindow = new PreviewWindow(logger: logger);

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

            // opt-in, off by default - see AppSettings.TelemetryEnabled remarks. Deliberately
            // only constructs FileTelemetryWriter (which creates the file immediately, even
            // before anything's written to it) when actually enabled, so a default install
            // never leaves a telemetry file behind at all. One file per launch, same naming
            // scheme as NLog.config's logfile target, so activation timings from any given run
            // are easy to find and compare against another.
            if (ConfigHelper.AppSettings?.TelemetryEnabled == true)
            {
                var telemetryPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "FancyMouse",
                    "Telemetry",
                    $"telemetry_{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.jsonl");
                Telemetry.Current.Start(new FileTelemetryWriter(telemetryPath));
            }

            logger.Info("starting hotkey handler");

            // owns making sure only one PreviewWindow.ShowPreviewAsync call is ever running at
            // a time - PreviewWindow itself is deliberately unaware of this; it just accepts a
            // CancellationToken and stops promptly when told to (see ShowPreviewAsync's
            // remarks). Rapid repeat activation (e.g. spam-pressing the hotkey) would otherwise
            // let two calls interleave and mutate the window's UI state at the same time, which
            // is visible as a brief white flash before the correct content redraws.
            CancellationTokenSource? activationCancellation = null;
            var activationTask = Task.CompletedTask;

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
                            activationCancellation?.Cancel();
                            try
                            {
                                await activationTask.ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                // expected - the previous activation observed the cancellation
                                // above and stopped
                            }

                            activationCancellation?.Dispose();
                            var cancellation = new CancellationTokenSource();
                            activationCancellation = cancellation;

                            var task = previewWindow.ShowPreviewAsync(cancellation.Token);
                            activationTask = task;
                            try
                            {
                                await task.ConfigureAwait(false);

                                // each activation churns through several WriteableBitmap-sized
                                // pixel buffers (background, bezels, content) - left to the GC's
                                // own schedule, those pile up as uncollected garbage quickly
                                // enough under repeat activation to show up as inflated memory
                                // usage in Task Manager. Collecting here - only on a successful,
                                // uninterrupted completion, i.e. the window is now shown and
                                // nothing superseded this activation - is the true end of user
                                // interaction, unlike ClearPreview's old GC.Collect() call, which
                                // ran at the *start* of every activation and competed with it for
                                // CPU. A background (non-blocking) collection avoids stalling this
                                // thread the way a blocking GC.Collect() + WaitForPendingFinalizers()
                                // would.
                                GC.Collect(2, GCCollectionMode.Optimized, blocking: false);
                            }
                            catch (OperationCanceledException)
                            {
                                // expected - superseded by a newer activation before this one
                                // finished
                            }
                            catch (Exception ex)
                            {
                                // this callback runs as a fire-and-forget async lambda passed to
                                // DispatcherQueue.TryEnqueue - anything other than
                                // OperationCanceledException would otherwise vanish silently
                                // instead of surfacing anywhere
                                logger.Error(ex, "unhandled exception while showing the preview window");
                            }
                        });
                });
            logger.Info("started hotkey handler");

            // create the system tray icon
            var trayIcon = new TrayIcon();
            trayIcon.ExitCommandClicked += (sender, e) =>
            {
                App.Current.Exit();
            };
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
