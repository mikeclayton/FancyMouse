using Microsoft.UI.Xaml;

using Application = Microsoft.UI.Xaml.Application;

namespace FancyMouse.SettingsUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();

        // this project has no logger of its own (unlike FancyMouse.WinUI3's NLog setup) - these
        // hooks exist purely so a startup crash writes *something* diagnosable to disk instead
        // of surfacing only as an opaque native exit code (e.g. 0xC000027B) with no stack trace,
        // which is otherwise very hard to debug since exceptions thrown this early don't always
        // reach the debugger's own exception-break machinery.
        this.UnhandledException += App.OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += App.OnCurrentDomainUnhandledException;
    }

    /// <summary>
    /// Gets or sets the main window instance so <see cref="Controls.ShortcutControl"/> can
    /// subscribe to <see cref="Window.Activated"/> - it needs to know when the settings window
    /// gains/loses focus so its keyboard hook can be disabled while some other window is active.
    /// </summary>
    internal Window? MainWindow
    {
        get;
        set;
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            // args[0], if present, overrides the default "look next to this exe" convention -
            // see MainWindow's remarks for why you'd want to point this at FancyMouse.WinUI3's
            // own running copy of appSettings.json instead of this exe's own local one.
            var commandLineArgs = Environment.GetCommandLineArgs();
            var appSettingsPath = (commandLineArgs.Length > 1) ? commandLineArgs[1] : ".\\appSettings.json";

            var window = new MainWindow(appSettingsPath);
            this.MainWindow = window;
            window.Activate();
        }
        catch (Exception ex)
        {
            App.LogCrash(ex);
            throw;
        }
    }

    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        => App.LogCrash(e.Exception);

    private static void OnCurrentDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        => App.LogCrash(e.ExceptionObject as Exception);

    private static void LogCrash(Exception? ex)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "crash.log");
            var details = new System.Text.StringBuilder();
            details.AppendLine(DateTime.Now.ToString("O"));

            var current = ex;
            var depth = 0;
            while (current is not null)
            {
                details.Append(System.Globalization.CultureInfo.InvariantCulture, $"--- depth {depth} ---").AppendLine();
                details.Append(System.Globalization.CultureInfo.InvariantCulture, $"Type: {current.GetType().FullName}").AppendLine();
                details.Append(System.Globalization.CultureInfo.InvariantCulture, $"HResult: 0x{current.HResult:X8}").AppendLine();
                details.Append(System.Globalization.CultureInfo.InvariantCulture, $"Message: {current.Message}").AppendLine();
                details.Append(System.Globalization.CultureInfo.InvariantCulture, $"StackTrace: {current.StackTrace}").AppendLine();
                foreach (var key in current.Data.Keys)
                {
                    details.Append(System.Globalization.CultureInfo.InvariantCulture, $"Data[{key}] = {current.Data[key]}").AppendLine();
                }

                current = current.InnerException;
                depth++;
            }

            details.AppendLine();
            File.AppendAllText(path, details.ToString());
        }
        catch
        {
            // logging the crash must never itself throw and mask the original exception
        }
    }
}
