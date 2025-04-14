using FancyMouse.WinUI3.Internal.Helpers;
using Microsoft.UI.Xaml;
using NLog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace FancyMouse.WinUI3
{
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

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // run Logitech SetPoint as admin for hotkeys to get activated from custom mouse bindings
            // when an Office application or Visual Studio is the active window. (SetPoint *keyboard*
            // bindings work fine when running as a normal user in Office, but *mouse* bindings only
            // work when SetPoint is run as an admin...)
            // https://social.msdn.microsoft.com/Forums/en-US/09a7ebee-9567-4704-be88-de54a16ca99e/logitech-mouse-button-assignments-ignored-by-vs?forum=csharpide
            var appSettingsPath = "C:\\src\\github\\mikeclayton\\FancyMouse\\src\\FancyMouse\\appSettings.json";
            ConfigHelper.SetAppSettingsPath(appSettingsPath);

            /* ConfigHelper.SetAppSettingsPath(".\\appSettings.json"); */

            var previewWindow = new PreviewWindow(
                logger: LogManager.CreateNullLogger());

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

            // load the application settings and start the filesystem watcher
            // so we reload if it changes
            ConfigHelper.LoadAppSettings();
            ConfigHelper.StartAppSettingsWatcher();
        }
    }
}
