using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FancyMouse.Models.Styles;
using FancyMouse.WindowsHotKeys;

namespace FancyMouse.Models.Settings.V1;

internal static class SettingsConverter
{
    public static AppSettings ParseAppSettings(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        var appConfig = JsonSerializer.Deserialize<AppConfig>(json, options)
            ?? throw new InvalidOperationException();
        var hotkey = SettingsConverter.ConvertToKeystroke(appConfig?.FancyMouse?.Hotkey);
        var previewStyle = SettingsConverter.ConvertToPreviewStyle(appConfig?.FancyMouse?.PreviewSize);
        var appSettings = new AppSettings(hotkey, previewStyle);
        return appSettings;
    }

    public static Keystroke ConvertToKeystroke(string? hotkey)
    {
        return (hotkey == null)
            ? AppSettings.DefaultSettings.Hotkey
            : Keystroke.Parse(hotkey);
    }

    public static PreviewStyle ConvertToPreviewStyle(string? previewSize)
    {
        if (previewSize is null)
        {
            return AppSettings.DefaultSettings.PreviewStyle;
        }

        var parts = previewSize.Split("x")
            .Select(part => int.Parse(part.Trim(), CultureInfo.InvariantCulture))
            .ToList();

        return new PreviewStyle(
            canvasSize: new(
                width: parts[0],
                height: parts[1]
            ),
            canvasStyle: AppSettings.DefaultSettings.PreviewStyle.CanvasStyle,
            screenshotStyle: AppSettings.DefaultSettings.PreviewStyle.ScreenshotStyle);
    }
}
