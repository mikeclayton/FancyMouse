using System.Drawing;
using System.Text.Json;

using FancyMouse.Models.Styles;
using FancyMouse.Settings.V1;

namespace FancyMouse.Settings.V2;

internal static class SettingsConverterV2
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static AppSettings ParseAppSettings(string json)
    {
        var jsonContext = new SerializationContextV2(SettingsConverterV2.JsonSerializerOptions);
        var appConfig = JsonSerializer.Deserialize<AppConfig>(json, jsonContext.AppConfig)
            ?? throw new InvalidOperationException();
        var hotkey = SettingsConverterV1.ConvertToKeystroke(appConfig.Hotkey);
        var previewStyle = SettingsConverterV2.MergePreviewStyles(appConfig.Preview, AppSettings.DefaultSettings.PreviewStyle);
        var appSettings = new AppSettings(hotkey, previewStyle);
        return appSettings;
    }

    public static PreviewStyle MergePreviewStyles(PreviewStyleSettings? previewStyle, PreviewStyle defaultStyle)
    {
        if (previewStyle is null)
        {
            return AppSettings.DefaultSettings.PreviewStyle;
        }

        return new PreviewStyle(
            canvasSize: new(
                width: SettingsConverterV2.Clamp(
                    value: previewStyle?.CanvasSize?.Width,
                    defaultValue: defaultStyle?.CanvasSize?.Width,
                    min: 50,
                    max: 99999),
                height: SettingsConverterV2.Clamp(
                    value: previewStyle?.CanvasSize?.Height,
                    defaultValue: defaultStyle?.CanvasSize?.Height,
                    min: 50,
                    max: 99999)
            ),
            canvasStyle: new(
                marginStyle: new(
                    all: 0
                ),
                borderStyle: SettingsConverterV2.MergeBorderStyles(
                    borderStyle: previewStyle?.CanvasStyle?.BorderStyle,
                    defaultStyle: defaultStyle?.CanvasStyle?.BorderStyle),
                paddingStyle: SettingsConverterV2.MergePaddingStyles(
                    paddingStyle: previewStyle?.CanvasStyle?.PaddingStyle,
                    defaultStyle: defaultStyle?.CanvasStyle?.PaddingStyle),
                backgroundStyle: new(
                    color1: SettingsConverterV2.MergeColors(
                        color: previewStyle?.CanvasStyle?.BackgroundStyle?.Color1,
                        defaultValue: defaultStyle?.CanvasStyle?.BackgroundStyle?.Color1),
                    color2: SettingsConverterV2.MergeColors(
                        color: previewStyle?.CanvasStyle?.BackgroundStyle?.Color2,
                        defaultValue: defaultStyle?.CanvasStyle?.BackgroundStyle?.Color2)
                )
            ),
            screenStyle: new(
                marginStyle: SettingsConverterV2.MergeMarginStyles(
                    marginStyle: previewStyle?.ScreenStyle?.MarginStyle,
                    defaultStyle: defaultStyle?.ScreenStyle?.MarginStyle),
                borderStyle: SettingsConverterV2.MergeBorderStyles(
                    borderStyle: previewStyle?.ScreenStyle?.BorderStyle,
                    defaultStyle: defaultStyle?.ScreenStyle?.BorderStyle),
                paddingStyle: new(
                    all: 0
                ),
                backgroundStyle: new(
                    color1: SettingsConverterV2.MergeColors(
                        color: previewStyle?.ScreenStyle?.BackgroundStyle?.Color1,
                        defaultValue: defaultStyle?.ScreenStyle?.BackgroundStyle?.Color1),
                    color2: SettingsConverterV2.MergeColors(
                        color: previewStyle?.ScreenStyle?.BackgroundStyle?.Color2,
                        defaultValue: defaultStyle?.ScreenStyle?.BackgroundStyle?.Color2)
                )
            ),
            extraColors: Array.Empty<Color>());
    }

    private static MarginStyle MergeMarginStyles(MarginStyleSettings? marginStyle, MarginStyle? defaultStyle)
    {
        return new(
            left: SettingsConverterV2.Clamp(
                value: marginStyle?.Width,
                defaultValue: defaultStyle?.Left,
                min: 0,
                max: 99),
            top: SettingsConverterV2.Clamp(
                value: marginStyle?.Width,
                defaultValue: defaultStyle?.Top,
                min: 0,
                max: 99),
            right: SettingsConverterV2.Clamp(
                value: marginStyle?.Width,
                defaultValue: defaultStyle?.Right,
                min: 0,
                max: 99),
            bottom: SettingsConverterV2.Clamp(
                value: marginStyle?.Width,
                defaultValue: defaultStyle?.Bottom,
                min: 0,
                max: 99)
        );
    }

    private static BorderStyle MergeBorderStyles(BorderStyleSettings? borderStyle, BorderStyle? defaultStyle)
    {
        return new(
            color: SettingsConverterV2.MergeColors(
                color: borderStyle?.Color,
                defaultValue: defaultStyle?.Color),
            left: SettingsConverterV2.Clamp(
                value: borderStyle?.Width,
                defaultValue: defaultStyle?.Left,
                min: 0,
                max: 99),
            top: SettingsConverterV2.Clamp(
                value: borderStyle?.Width,
                defaultValue: defaultStyle?.Top,
                min: 0,
                max: 99),
            right: SettingsConverterV2.Clamp(
                value: borderStyle?.Width,
                defaultValue: defaultStyle?.Right,
                min: 0,
                max: 99),
            bottom: SettingsConverterV2.Clamp(
                value: borderStyle?.Width,
                defaultValue: defaultStyle?.Bottom,
                min: 0,
                max: 99),
            depth: SettingsConverterV2.Clamp(
                value: borderStyle?.Depth,
                defaultValue: defaultStyle?.Depth,
                min: 0,
                max: 99)
        );
    }

    private static PaddingStyle MergePaddingStyles(PaddingStyleSettings? paddingStyle, PaddingStyle? defaultStyle)
    {
        return new(
            left: SettingsConverterV2.Clamp(
                value: paddingStyle?.Width,
                defaultValue: defaultStyle?.Left,
                min: 0,
                max: 99),
            top: SettingsConverterV2.Clamp(
                value: paddingStyle?.Width,
                defaultValue: defaultStyle?.Top,
                min: 0,
                max: 99),
            right: SettingsConverterV2.Clamp(
                value: paddingStyle?.Width,
                defaultValue: defaultStyle?.Right,
                min: 0,
                max: 99),
            bottom: SettingsConverterV2.Clamp(
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
