using System.Globalization;
using FancyMouse.Models.Styles;

namespace FancyMouse.Models.Settings;

internal static class SettingsConverter
{
    public static PreviewStyle ConvertToPreviewStyle(PreviewSettings previewSettings)
    {
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
                        @default: Color.Red),
                    all: Math.Clamp(previewSettings.CanvasStyle.BorderStyle.Width, 0, 99),
                    depth: Math.Clamp(previewSettings.CanvasStyle.BorderStyle.Depth, 0, 99)
                ),
                paddingStyle: new(
                    all: Math.Clamp(previewSettings.CanvasStyle.PaddingStyle.Width, 0, 99)
                ),
                backgroundStyle: new(
                    color1: SettingsConverter.ParseColorSettings(
                        value: previewSettings.CanvasStyle.BackgroundStyle.Color1,
                        @default: Color.Red),
                    color2: SettingsConverter.ParseColorSettings(
                        value: previewSettings.CanvasStyle.BackgroundStyle.Color2,
                        @default: Color.Red)
                )
            ),
            screenshotStyle: new(
                marginStyle: new(
                    Math.Clamp(previewSettings.ScreenshotStyle.MarginStyle.Width, 0, 99)
                ),
                borderStyle: new(
                    color: SettingsConverter.ParseColorSettings(
                        value: previewSettings.ScreenshotStyle.BorderStyle.Color,
                        @default: Color.Red),
                    all: previewSettings.ScreenshotStyle.BorderStyle.Width,
                    depth: previewSettings.ScreenshotStyle.BorderStyle.Depth
                ),
                paddingStyle: PaddingStyle.Empty,
                backgroundStyle: new(
                    color1: SettingsConverter.ParseColorSettings(
                        previewSettings.ScreenshotStyle.BackgroundStyle.Color1,
                        @default: Color.Red),
                    color2: SettingsConverter.ParseColorSettings(
                        value: previewSettings.ScreenshotStyle.BackgroundStyle.Color2,
                        @default: Color.Red)
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
