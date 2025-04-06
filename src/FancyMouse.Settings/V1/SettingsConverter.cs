using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FancyMouse.Common.Models.Styles;
using FancyMouse.HotKeys;

namespace FancyMouse.Settings.V1;

internal static class SettingsConverter
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static Settings.AppSettings ParseAppSettings(string json)
    {
        var appConfig = JsonSerializer.Deserialize<AppConfig>(json, SettingsConverter.JsonSerializerOptions)
            ?? throw new InvalidOperationException();
        var hotkey = SettingsConverter.ConvertToKeystroke(appConfig?.FancyMouse?.Hotkey);
        var previewStyle = SettingsConverter.ConvertToPreviewStyle(appConfig?.FancyMouse?.PreviewSize);
        var appSettings = new Settings.AppSettings(hotkey, previewStyle);
        return appSettings;
    }

    public static Keystroke ConvertToKeystroke(string? hotkey)
    {
        return (hotkey == null)
            ? Settings.AppSettings.DefaultSettings.Hotkey
            : Keystroke.Parse(hotkey);
    }

    public static PreviewStyle ConvertToPreviewStyle(string? previewSize)
    {
        if (previewSize is null)
        {
            return Settings.AppSettings.DefaultSettings.PreviewStyle;
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
            screenStyle: AppSettings.DefaultSettings.PreviewStyle.ScreenStyle,
            mwbColors: AppSettings.DefaultSettings.PreviewStyle.MwbColors);
    }
}
