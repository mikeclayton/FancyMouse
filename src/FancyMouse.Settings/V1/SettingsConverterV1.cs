using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using FancyMouse.HotKeys;
using FancyMouse.Models.Styles;
using FancyMouse.Settings.V2;

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

        // v1 predates the Compact/Bezelled/Custom split entirely - a v1 config with an actual
        // preview size configured is, and always renders as, a fully custom style; one with
        // nothing configured falls back to the same default PreviewType a totally fresh install
        // gets, the same way previewStyle/hotkey already fall back to AppSettings.DefaultSettings.
        var previewType = (appConfig.FancyMouse?.PreviewSize is not null)
            ? PreviewType.Custom
            : AppSettings.DefaultSettings.PreviewType;
        var appSettings = new AppSettings(hotkey, previewStyle, previewType, telemetryEnabled: false);
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
