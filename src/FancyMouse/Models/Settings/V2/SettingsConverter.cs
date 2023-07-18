using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FancyMouse.Models.Styles;
using BorderStyle = FancyMouse.Models.Styles.BorderStyle;

namespace FancyMouse.Models.Settings.V2;

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
        var hotkey = V1.SettingsConverter.ConvertToKeystroke(appConfig.Hotkey);
        var previewStyle = SettingsConverter.ConvertToPreviewStyle(appConfig.Preview);
        var appSettings = new AppSettings(hotkey, previewStyle);
        return appSettings;
    }

    public static PreviewStyle ConvertToPreviewStyle(PreviewSettings? previewSettings)
    {
        if (previewSettings is null)
        {
            return AppSettings.DefaultSettings.PreviewStyle;
        }

        var defaultStyle = AppSettings.DefaultSettings.PreviewStyle;
        var canvasStyle = AppSettings.DefaultSettings.PreviewStyle.CanvasStyle;
        var screenshotStyle = AppSettings.DefaultSettings.PreviewStyle.ScreenshotStyle;
        return new PreviewStyle(
            canvasSize: new(
                width: Math.Clamp(
                    value: previewSettings?.CanvasSize?.Width ?? defaultStyle.CanvasSize.Width,
                    min: 50,
                    max: 99999),
                height: Math.Clamp(
                    value: previewSettings?.CanvasSize?.Height ?? defaultStyle.CanvasSize.Height,
                    min: 50,
                    max: 99999)
            ),
            canvasStyle: new(
                marginStyle: MarginStyle.Empty,
                borderStyle: SettingsConverter.ConvertToBorderStyle(previewSettings?.CanvasStyle?.BorderStyle, defaultStyle.CanvasStyle.BorderStyle),
                paddingStyle: new(
                    all: Math.Clamp(
                        value: previewSettings?.CanvasStyle?.PaddingStyle?.Width ?? canvasStyle.PaddingStyle.Top,
                        min: 0,
                        max: 99)
                ),
                backgroundStyle: new(
                    color1: SettingsConverter.ParseColorSettings(
                        value: previewSettings?.CanvasStyle?.BackgroundStyle?.Color1,
                        defaultValue: canvasStyle.BackgroundStyle.Color1),
                    color2: SettingsConverter.ParseColorSettings(
                        value: previewSettings?.CanvasStyle?.BackgroundStyle?.Color2,
                        defaultValue: canvasStyle.BackgroundStyle.Color2)
                )
            ),
            screenshotStyle: new(
                marginStyle: new(
                    Math.Clamp(
                        value: previewSettings?.ScreenshotStyle?.MarginStyle?.Width ?? screenshotStyle.MarginStyle.Top,
                        min: 0,
                        max: 99)
                ),
                borderStyle: SettingsConverter.ConvertToBorderStyle(previewSettings?.ScreenshotStyle?.BorderStyle, defaultStyle.CanvasStyle.BorderStyle),
                paddingStyle: PaddingStyle.Empty,
                backgroundStyle: new(
                    color1: SettingsConverter.ParseColorSettings(
                        previewSettings?.ScreenshotStyle?.BackgroundStyle?.Color1,
                        defaultValue: screenshotStyle.BackgroundStyle.Color1),
                    color2: SettingsConverter.ParseColorSettings(
                        value: previewSettings?.ScreenshotStyle?.BackgroundStyle?.Color2,
                        defaultValue: screenshotStyle.BackgroundStyle.Color2)
                )
            ));
    }

    private static BorderStyle ConvertToBorderStyle(BorderStyleSettings? settings, BorderStyle defaultStyle)
    {
        return new(
            color: settings?.Color is null
                ? defaultStyle.Color
                : SettingsConverter.ParseColorSettings(
                    value: settings.Color,
                    defaultValue: defaultStyle.Color),
            all: Math.Clamp(
                value: settings?.Width ?? defaultStyle.Top,
                min: 0,
                max: 99),
            depth: Math.Clamp(
                value: settings?.Depth ?? defaultStyle.Depth,
                min: 0,
                max: 99)
        );
    }

    private static Color ParseColorSettings(string? value, Color defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        var comparison = StringComparison.InvariantCulture;
        if (value.StartsWith("#", comparison))
        {
            var culture = CultureInfo.InvariantCulture;
            if ((value.Length == 7)
               && int.TryParse(value[1..3], NumberStyles.HexNumber, culture, out var r)
               && int.TryParse(value[3..5], NumberStyles.HexNumber, culture, out var g)
               && int.TryParse(value[5..7], NumberStyles.HexNumber, culture, out var b))
            {
                return Color.FromArgb(0xff, r, g, b);
            }
        }

        if (value.StartsWith("Color.", comparison))
        {
            var propertyName = value["Color.".Length..];
            var property = typeof(Color).GetProperties()
                .SingleOrDefault(property => property.Name == propertyName);
            if (property is not null)
            {
                var propertyValue = property.GetValue(null, null);
                return (propertyValue is null) ? defaultValue : (Color)propertyValue;
            }
        }

        if (value.StartsWith("SystemColors.", comparison))
        {
            var propertyName = value["SystemColors.".Length..];
            var property = typeof(SystemColors).GetProperties()
                .SingleOrDefault(property => property.Name == propertyName);
            if (property is not null)
            {
                var propertyValue = property.GetValue(null, null);
                return (propertyValue is null) ? defaultValue : (Color)propertyValue;
            }
        }

        return defaultValue;
    }
}
