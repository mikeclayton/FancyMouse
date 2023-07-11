using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FancyMouse.Models.Styles;
using FancyMouse.WindowsHotKeys;

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
        var appSettings = new AppSettings(
            hotkey: SettingsConverter.ConvertToKeystroke(appConfig.Hotkey),
            previewStyle: SettingsConverter.ConvertToPreviewStyle(appConfig.Preview));
        return appSettings;
    }

    public static Keystroke ConvertToKeystroke(HotkeySettings? hotkeySettings)
    {
        if (hotkeySettings is null)
        {
            return AppSettings.DefaultSettings.Hotkey;
        }

        var key = Enum.TryParse<WindowsHotKeys.Keys>(hotkeySettings.Key, out var outKey)
            ? outKey
            : AppSettings.DefaultSettings.Hotkey.Key;
        var modifiers = Enum.TryParse<KeyModifiers>(hotkeySettings.Modifiers, out var outModifiers)
            ? outModifiers
            : AppSettings.DefaultSettings.Hotkey.Modifiers;
        return new Keystroke(key, modifiers);
    }

    public static PreviewStyle ConvertToPreviewStyle(PreviewSettings? previewSettings)
    {
        if (previewSettings is null)
        {
            return AppSettings.DefaultSettings.PreviewStyle;
        }

        var canvasStyle = AppSettings.DefaultSettings.PreviewStyle.CanvasStyle;
        var screenshotStyle = AppSettings.DefaultSettings.PreviewStyle.ScreenshotStyle;
        return new PreviewStyle(
            canvasSize: new(
                width: Math.Clamp(previewSettings.CanvasSize.Width, 50, 99999),
                height: Math.Clamp(previewSettings.CanvasSize.Height, 50, 99999)
            ),
            canvasStyle: new(
                marginStyle: MarginStyle.Empty,
                borderStyle: new(
                    color: SettingsConverter.ParseColorSettings(
                        value: previewSettings.CanvasStyle.BorderStyle.Color,
                        @default: canvasStyle.BorderStyle.Color),
                    all: Math.Clamp(previewSettings.CanvasStyle.BorderStyle.Width, 0, 99),
                    depth: Math.Clamp(previewSettings.CanvasStyle.BorderStyle.Depth, 0, 99)
                ),
                paddingStyle: new(
                    all: Math.Clamp(previewSettings.CanvasStyle.PaddingStyle.Width, 0, 99)
                ),
                backgroundStyle: new(
                    color1: SettingsConverter.ParseColorSettings(
                        value: previewSettings.CanvasStyle.BackgroundStyle.Color1,
                        @default: canvasStyle.BackgroundStyle.Color1),
                    color2: SettingsConverter.ParseColorSettings(
                        value: previewSettings.CanvasStyle.BackgroundStyle.Color2,
                        @default: canvasStyle.BackgroundStyle.Color2)
                )
            ),
            screenshotStyle: new(
                marginStyle: new(
                    Math.Clamp(previewSettings.ScreenshotStyle.MarginStyle.Width, 0, 99)
                ),
                borderStyle: new(
                    color: SettingsConverter.ParseColorSettings(
                        value: previewSettings.ScreenshotStyle.BorderStyle.Color,
                        @default: screenshotStyle.BorderStyle.Color),
                    all: previewSettings.ScreenshotStyle.BorderStyle.Width,
                    depth: previewSettings.ScreenshotStyle.BorderStyle.Depth
                ),
                paddingStyle: PaddingStyle.Empty,
                backgroundStyle: new(
                    color1: SettingsConverter.ParseColorSettings(
                        previewSettings.ScreenshotStyle.BackgroundStyle.Color1,
                        @default: screenshotStyle.BackgroundStyle.Color1),
                    color2: SettingsConverter.ParseColorSettings(
                        value: previewSettings.ScreenshotStyle.BackgroundStyle.Color2,
                        @default: screenshotStyle.BackgroundStyle.Color2)
                )
            ));
    }

    public static Color ParseColorSettings(string value, Color @default)
    {
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
            var propertyName = value.Substring("Color.".Length);
            var property = typeof(Color).GetProperties()
                .SingleOrDefault(property => property.Name == propertyName);
            if (property is not null)
            {
                var propertyValue = property.GetValue(null, null);
                return (propertyValue is null) ? @default : (Color)propertyValue;
            }
        }

        if (value.StartsWith("SystemColors.", comparison))
        {
            var propertyName = value.Substring("SystemColors.".Length);
            var property = typeof(SystemColors).GetProperties()
                .SingleOrDefault(property => property.Name == propertyName);
            if (property is not null)
            {
                var propertyValue = property.GetValue(null, null);
                return (propertyValue is null) ? @default : (Color)propertyValue;
            }
        }

        return @default;
    }
}
