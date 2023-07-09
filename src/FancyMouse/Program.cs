using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Settings;
using FancyMouse.UI;
using FancyMouse.WindowsHotKeys;
using Microsoft.Extensions.Configuration;
using NLog;
using Keys = FancyMouse.WindowsHotKeys.Keys;

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

        /*
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        var appSettings = JsonSerializer.Deserialize<AppSettings>(
            File.ReadAllText("appSettings.json"), options)
            ?? throw new InvalidOperationException();
        */

        var legacySettings = new AppSettings(
            hotkey: new(
                key: Keys.F,
                modifiers: KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift
            ),
            preview: new(
                size: new(1600, 1200),
                previewStyle: new(
                    marginInfo: MarginInfo.Empty,
                    borderInfo: new(
                        color: SystemColors.Highlight,
                        all: 5,
                        depth: 0
                    ),
                    paddingInfo: new(0),
                    backgroundInfo: new(
                        color1: Color.FromArgb(13, 87, 210), // light blue
                        color2: Color.FromArgb(3, 68, 192) // darker blue
                    )
                ),
                screenshotStyle: BoxStyle.Empty
            ));

        var defaultSettings = new AppSettings(
            hotkey: new(
                key: Keys.F,
                modifiers: KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift
            ),
            preview: new(
                size: new(1600, 1200),
                previewStyle: new(
                    marginInfo: MarginInfo.Empty,
                    borderInfo: new(
                        color: SystemColors.Highlight,
                        all: 7,
                        depth: 0
                    ),
                    paddingInfo: new(15),
                    backgroundInfo: new(
                        color1: Color.FromArgb(13, 87, 210), // light blue
                        color2: Color.FromArgb(3, 68, 192) // darker blue
                    )
                ),
                screenshotStyle: new(
                    marginInfo: new(1),
                    borderInfo: new(
                        color: Color.FromArgb(0xFF, 0x22, 0x22, 0x22), // dark grey
                        all: 15,
                        depth: 2
                    ),
                    paddingInfo: PaddingInfo.Empty,
                    backgroundInfo: new(
                        Color.MidnightBlue,
                        Color.MidnightBlue
                    )
                )));

        var spacedSettings = new AppSettings(
            hotkey: new(
                key: Keys.F,
                modifiers: KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift
            ),
            preview: new(
                size: new(800, 600),
                previewStyle: new(
                    marginInfo: MarginInfo.Empty,
                    borderInfo: new(
                        color: SystemColors.Highlight,
                        all: 15,
                        depth: 4
                    ),
                    paddingInfo: new(15),
                    backgroundInfo: new(
                        color1: Color.FromArgb(13, 87, 210), // light blue
                        color2: Color.FromArgb(3, 68, 192) // darker blue
                    )
                ),
                screenshotStyle: new(
                    marginInfo: new(25),
                    borderInfo: BorderInfo.Empty,
                    paddingInfo: PaddingInfo.Empty,
                    backgroundInfo: new(
                        Color.MidnightBlue,
                        Color.MidnightBlue
                    )
                )));

        var gaudy1Settings = new AppSettings(
            hotkey: new(
                key: Keys.F,
                modifiers: KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift
            ),
            preview: new(
                size: new(400, 300),
                previewStyle: new(
                    marginInfo: MarginInfo.Empty,
                    borderInfo: new(
                        color: Color.Red,
                        all: 7,
                        depth: 2
                    ),
                    paddingInfo: new(15),
                    backgroundInfo: new(
                        color1: Color.Yellow,
                        color2: Color.Green
                    )),
                screenshotStyle: new(
                    marginInfo: new(1),
                    borderInfo: new(
                        color: Color.HotPink,
                        all: 15,
                        depth: 4
                    ),
                    paddingInfo: PaddingInfo.Empty,
                    backgroundInfo: new(
                        Color.MidnightBlue,
                        Color.MidnightBlue
                    )
                )));

        var appSettings = new[]
        {
            legacySettings,
            defaultSettings,
            spacedSettings,
            gaudy1Settings,
        }.Skip(3).First();

        // logger: LogManager.LoadConfiguration(".\\NLog.config").GetCurrentClassLogger(),
        var dialog = new FancyMouseDialog(
            new FancyMouseDialogOptions(
                logger: LogManager.CreateNullLogger(),
                previewSettings: appSettings.Preview));

        var hotKeyManager = new HotKeyManager(appSettings.Hotkey);
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
