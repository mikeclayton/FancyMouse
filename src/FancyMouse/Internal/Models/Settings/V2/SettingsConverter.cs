using System.Text.Json;
using FancyMouse.Common.Models.Styles;
using BorderStyle = FancyMouse.Common.Models.Styles.BorderStyle;

namespace FancyMouse.Internal.Models.Settings.V2;

internal static class SettingsConverter
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static Settings.AppSettings ParseAppSettings(string json)
    {
        var appConfig = JsonSerializer.Deserialize<AppConfig>(json, SettingsConverter.JsonSerializerOptions)
            ?? throw new InvalidOperationException();
        var hotkey = V1.SettingsConverter.ConvertToKeystroke(appConfig.Hotkey);
        var previewStyle = SettingsConverter.MergePreviewStyles(appConfig.Preview, Settings.AppSettings.DefaultSettings.PreviewStyle);
        var appSettings = new Settings.AppSettings(hotkey, previewStyle);
        return appSettings;
    }

    public static PreviewStyle MergePreviewStyles(PreviewStyleSettings? previewStyle, PreviewStyle defaultStyle)
    {
        if (previewStyle is null)
        {
            return Settings.AppSettings.DefaultSettings.PreviewStyle;
        }

        return new PreviewStyle(
            canvasSize: new(
                width: SettingsConverter.Clamp(
                    value: previewStyle?.CanvasSize?.Width,
                    defaultValue: defaultStyle?.CanvasSize?.Width,
                    min: 50,
                    max: 99999),
                height: SettingsConverter.Clamp(
                    value: previewStyle?.CanvasSize?.Height,
                    defaultValue: defaultStyle?.CanvasSize?.Height,
                    min: 50,
                    max: 99999)
            ),
            canvasStyle: new(
                marginStyle: new(
                    all: 0
                ),
                borderStyle: SettingsConverter.MergeBorderStyles(
                    borderStyle: previewStyle?.CanvasStyle?.BorderStyle,
                    defaultStyle: defaultStyle?.CanvasStyle?.BorderStyle),
                paddingStyle: SettingsConverter.MergePaddingStyles(
                    paddingStyle: previewStyle?.CanvasStyle?.PaddingStyle,
                    defaultStyle: defaultStyle?.CanvasStyle?.PaddingStyle),
                backgroundStyle: new(
                    color1: SettingsConverter.MergeColors(
                        color: previewStyle?.CanvasStyle?.BackgroundStyle?.Color1,
                        defaultValue: defaultStyle?.CanvasStyle?.BackgroundStyle?.Color1),
                    color2: SettingsConverter.MergeColors(
                        color: previewStyle?.CanvasStyle?.BackgroundStyle?.Color2,
                        defaultValue: defaultStyle?.CanvasStyle?.BackgroundStyle?.Color2)
                )
            ),
            screenStyle: new(
                marginStyle: SettingsConverter.MergeMarginStyles(
                    marginStyle: previewStyle?.ScreenStyle?.MarginStyle,
                    defaultStyle: defaultStyle?.ScreenStyle?.MarginStyle),
                borderStyle: SettingsConverter.MergeBorderStyles(
                    borderStyle: previewStyle?.ScreenStyle?.BorderStyle,
                    defaultStyle: defaultStyle?.ScreenStyle?.BorderStyle),
                paddingStyle: new(
                    all: 0
                ),
                backgroundStyle: new(
                    color1: SettingsConverter.MergeColors(
                        color: previewStyle?.ScreenStyle?.BackgroundStyle?.Color1,
                        defaultValue: defaultStyle?.ScreenStyle?.BackgroundStyle?.Color1),
                    color2: SettingsConverter.MergeColors(
                        color: previewStyle?.ScreenStyle?.BackgroundStyle?.Color2,
                        defaultValue: defaultStyle?.ScreenStyle?.BackgroundStyle?.Color2)
                )
            ));
    }

    private static MarginStyle MergeMarginStyles(MarginStyleSettings? marginStyle, MarginStyle? defaultStyle)
    {
        return new(
            left: SettingsConverter.Clamp(
                value: marginStyle?.Width,
                defaultValue: defaultStyle?.Left,
                min: 0,
                max: 99),
            top: SettingsConverter.Clamp(
                value: marginStyle?.Width,
                defaultValue: defaultStyle?.Top,
                min: 0,
                max: 99),
            right: SettingsConverter.Clamp(
                value: marginStyle?.Width,
                defaultValue: defaultStyle?.Right,
                min: 0,
                max: 99),
            bottom: SettingsConverter.Clamp(
                value: marginStyle?.Width,
                defaultValue: defaultStyle?.Bottom,
                min: 0,
                max: 99)
        );
    }

    private static BorderStyle MergeBorderStyles(BorderStyleSettings? borderStyle, BorderStyle? defaultStyle)
    {
        return new(
            color: SettingsConverter.MergeColors(
                color: borderStyle?.Color,
                defaultValue: defaultStyle?.Color),
            left: SettingsConverter.Clamp(
                value: borderStyle?.Width,
                defaultValue: defaultStyle?.Left,
                min: 0,
                max: 99),
            top: SettingsConverter.Clamp(
                value: borderStyle?.Width,
                defaultValue: defaultStyle?.Top,
                min: 0,
                max: 99),
            right: SettingsConverter.Clamp(
                value: borderStyle?.Width,
                defaultValue: defaultStyle?.Right,
                min: 0,
                max: 99),
            bottom: SettingsConverter.Clamp(
                value: borderStyle?.Width,
                defaultValue: defaultStyle?.Bottom,
                min: 0,
                max: 99),
            depth: SettingsConverter.Clamp(
                value: borderStyle?.Depth,
                defaultValue: defaultStyle?.Depth,
                min: 0,
                max: 99)
        );
    }

    private static PaddingStyle MergePaddingStyles(PaddingStyleSettings? paddingStyle, PaddingStyle? defaultStyle)
    {
        return new(
            left: SettingsConverter.Clamp(
                value: paddingStyle?.Width,
                defaultValue: defaultStyle?.Left,
                min: 0,
                max: 99),
            top: SettingsConverter.Clamp(
                value: paddingStyle?.Width,
                defaultValue: defaultStyle?.Top,
                min: 0,
                max: 99),
            right: SettingsConverter.Clamp(
                value: paddingStyle?.Width,
                defaultValue: defaultStyle?.Right,
                min: 0,
                max: 99),
            bottom: SettingsConverter.Clamp(
                value: paddingStyle?.Width,
                defaultValue: defaultStyle?.Bottom,
                min: 0,
                max: 99)
        );
    }

    private static Color MergeColors(string? color, Color? defaultValue)
    {
        return ColorConverter.Deserialize(color) ?? defaultValue ?? throw new InvalidOperationException();
    }

    private static decimal Clamp(decimal? value, decimal? defaultValue, decimal min, decimal max)
    {
        return (value.HasValue || defaultValue.HasValue)
            ? Math.Clamp(value ?? defaultValue ?? throw new InvalidOperationException(), min, max)
            : throw new InvalidOperationException();
    }
}
