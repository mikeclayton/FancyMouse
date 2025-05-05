using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using FancyMouse.HotKeys;
using FancyMouse.Models.Styles;

namespace FancyMouse.Settings.V1;

internal static class SettingsConverterV1
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
    };

    private static readonly SerializationContextV1 JsonSerializationContext = new(SettingsConverterV1.JsonSerializerOptions);

    public static AppSettings ParseAppSettings(string json)
    {
        var appConfig = JsonSerializer.Deserialize<AppConfig>(json, SettingsConverterV1.JsonSerializationContext.AppConfig)
            ?? throw new InvalidOperationException();
        var hotkey = SettingsConverterV1.ConvertToKeystroke(appConfig.FancyMouse?.Hotkey);
        var previewStyle = SettingsConverterV1.ConvertToPreviewStyle(appConfig.FancyMouse?.PreviewSize);
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
            screenStyle: AppSettings.DefaultSettings.PreviewStyle.ScreenStyle,
            extraColors: AppSettings.DefaultSettings.PreviewStyle.ExtraColors);
    }
}
