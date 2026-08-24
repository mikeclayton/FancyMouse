using FancyMouse.Common.Telemetry;

using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using NLog;

namespace FancyMouse.WinUI3;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var logger = LogManager.CreateNullLogger();

        WinRT.ComWrappersSupport.InitializeComWrappers();

        var instanceKey = AppInstance.FindOrRegisterForKey("FancyMouse_Instance");
        if (instanceKey.IsCurrent)
        {
            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }
        else
        {
            logger.Warn("another instance is running. exiting");
        }

        // Application.Start blocks until the message loop actually ends (e.g. the tray icon's
        // Exit command calling App.Current.Exit()), so this is the one place a graceful shutdown
        // reliably runs - unlike stopping the debugger, which kills the process outright and
        // can't be flushed around. Stop() drains whatever's still queued and flushes/disposes
        // the underlying writer (see TelemetryAdapter.Dispose).
        Telemetry.Current.Stop();

        return;
    }
}
