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

        return;
    }
}
