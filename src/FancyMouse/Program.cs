using System.Globalization;
using FancyMouse.UI;
using FancyMouse.WindowsHotKeys;
using Microsoft.Extensions.Configuration;
using NLog;

namespace FancyMouse;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // run Logitech SetPoint as admin for hotkeys to get activated from custom mouse bindings
        // when an Office application or Visual Studio is the active window. (SetPoint *keyboard*
        // bindings work fine when running as a normal user in Office, but *mouse* bindings only
        // work when SetPoint is run as an admin...)
        // https://social.msdn.microsoft.com/Forums/en-US/09a7ebee-9567-4704-be88-de54a16ca99e/logitech-mouse-button-assignments-ignored-by-vs?forum=csharpide

        // scheduled task to start app at logon

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        if (Application.HighDpiMode != HighDpiMode.PerMonitorV2)
        {
            throw new InvalidOperationException("high dpi mode is not set to PerMonitorV2");
        }

        // create the notify icon for the application
        var notifyForm = new FancyMouseNotify();

        var config = new ConfigurationBuilder()
            .AddJsonFile("FancyMouse.json")
            .Build()
            .GetSection("FancyMouse");

        var preview = (config["Preview"] ?? throw new InvalidOperationException("Missing config value 'Preview'"))
            .Split("x").Select(s => int.Parse(s.Trim(), CultureInfo.InvariantCulture)).ToList();

        // logger: LogManager.LoadConfiguration(".\\NLog.config").GetCurrentClassLogger(),
        var dialog = new FancyMouseDialog(
            new FancyMouseDialogOptions(
                logger: LogManager.CreateNullLogger(),
                maximumThumbnailSize: new Size(
                    preview[0], preview[1])));

        var hotkey = Keystroke.Parse(
            config["HotKey"] ?? throw new InvalidOperationException("Missing config value 'HotKey'"));
        var hotKeyManager = new HotKeyManager(hotkey);
        hotKeyManager.HotKeyPressed +=
            (_, _) =>
            {
                dialog.Show();
            };
        hotKeyManager.Start();

        Application.Run();

        hotKeyManager.Stop();
    }
}
