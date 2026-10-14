using System.Drawing;
using System.Text.Json;

using FancyMouse.Common.Helpers;
using FancyMouse.Models.Styles;
using FancyMouse.Settings.V1;

using ColorConverter = FancyMouse.Settings.V2.Converters.ColorConverter;

namespace FancyMouse.Settings.V2;

public static class SettingsConverterV2
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly SerializationContextV2 JsonSerializationContext = new(SettingsConverterV2.JsonSerializerOptions);

    public static AppSettings ParseAppSettings(string json)
    {
        var appConfig = JsonSerializer.Deserialize<AppConfig>(json, SettingsConverterV2.JsonSerializationContext.AppConfig)
            ?? throw new InvalidOperationException();
        var hotkey = SettingsConverterV1.ConvertToKeystroke(appConfig.Hotkey);

        // AppSettings.PreviewStyle is a *settings* representation - it always carries the user's
        // full custom style, regardless of Type, so it merges rather than resolves. See
        // AppSettings' own remarks.
        var previewStyle = SettingsConverterV2.MergePreviewStyles(appConfig.Preview, AppSettings.DefaultSettings.PreviewStyle);
        var previewType = SettingsConverterV2.ParsePreviewType(appConfig.Preview?.Type);

        var telemetryEnabled = appConfig.Telemetry?.Enabled ?? false;
        var appSettings = new AppSettings(hotkey, previewStyle, previewType, telemetryEnabled);
        return appSettings;
    }

    /// <summary>
    /// Parses the persisted "type" string into a <see cref="PreviewType"/>, defaulting to
    /// <see cref="PreviewType.Bezelled"/> for a missing or unrecognised value, matching
    /// MouseJump's own default.
    /// </summary>
    public static PreviewType ParsePreviewType(string? typeName)
    {
        return Enum.TryParse<PreviewType>(typeName, ignoreCase: true, out var parsed)
            ? parsed
            : PreviewType.Bezelled;
    }

    /// <summary>
    /// The equivalent of PowerToys' own <c>SettingsHelper.GetActivePreviewStyle</c> - derives the
    /// *visual* representation to render from a *settings* representation
    /// (<paramref name="previewStyle"/>/<paramref name="previewType"/>, e.g.
    /// <see cref="AppSettings.PreviewStyle"/>/<see cref="AppSettings.PreviewType"/>): copies
    /// <paramref name="previewStyle"/>'s canvas size onto whichever of
    /// <see cref="StyleHelper.CompactPreviewStyle"/>/<see cref="StyleHelper.BezelledPreviewStyle"/>
    /// is selected, or returns <paramref name="previewStyle"/> itself unchanged for Custom. Never
    /// mutates <paramref name="previewStyle"/> - call this at the point of rendering, not when
    /// parsing/loading settings, so the settings representation it derives from is never itself
    /// overwritten by a preset's values.
    /// </summary>
    public static PreviewStyle GetActivePreviewStyle(PreviewStyle previewStyle, PreviewType previewType)
    {
        return previewType switch
        {
            PreviewType.Compact => StyleHelper.CompactPreviewStyle.WithCanvasSize(previewStyle.CanvasSize),
            PreviewType.Bezelled => StyleHelper.BezelledPreviewStyle.WithCanvasSize(previewStyle.CanvasSize),
            PreviewType.Custom => previewStyle,
            _ => throw new InvalidOperationException($"Unhandled {nameof(PreviewType)} '{previewType}'"),
        };
    }

    public static PreviewStyle MergePreviewStyles(PreviewStyleSettings? previewStyle, PreviewStyle defaultStyle)
    {
        if (previewStyle is null)
        {
            return AppSettings.DefaultSettings.PreviewStyle;
        }

        // extraColors has no JSON representation yet, so it always inherits the default
        return new PreviewStyle(
            canvasSize: new(
                width: SettingsConverterV2.Clamp(
                    value: previewStyle.CanvasSize?.Width,
                    defaultValue: defaultStyle.CanvasSize?.Width,
                    min: 50,
                    max: 99999),
                height: SettingsConverterV2.Clamp(
                    value: previewStyle.CanvasSize?.Height,
                    defaultValue: defaultStyle.CanvasSize?.Height,
                    min: 50,
                    max: 99999)
            ),
            canvasStyle: new(
                marginStyle: new(
                    all: 0
                ),
                borderStyle: SettingsConverterV2.MergeBorderStyles(
                    borderStyle: previewStyle.CanvasStyle?.BorderStyle,
                    defaultStyle: defaultStyle.CanvasStyle?.BorderStyle),
                paddingStyle: SettingsConverterV2.MergePaddingStyles(
                    paddingStyle: previewStyle.CanvasStyle?.PaddingStyle,
                    defaultStyle: defaultStyle.CanvasStyle?.PaddingStyle),
                backgroundStyle: new(
                    color1: SettingsConverterV2.MergeColors(
                        color: previewStyle.CanvasStyle?.BackgroundStyle?.Color1,
                        defaultValue: defaultStyle.CanvasStyle?.BackgroundStyle?.Color1),
                    color2: SettingsConverterV2.MergeColors(
                        color: previewStyle.CanvasStyle?.BackgroundStyle?.Color2,
                        defaultValue: defaultStyle.CanvasStyle?.BackgroundStyle?.Color2)
                )
            ),
            screenStyle: new(
                marginStyle: SettingsConverterV2.MergeMarginStyles(
                    marginStyle: previewStyle.ScreenStyle?.MarginStyle,
                    defaultStyle: defaultStyle.ScreenStyle?.MarginStyle),
                borderStyle: SettingsConverterV2.MergeBorderStyles(
                    borderStyle: previewStyle.ScreenStyle?.BorderStyle,
                    defaultStyle: defaultStyle.ScreenStyle?.BorderStyle),
                paddingStyle: new(
                    all: 0
                ),
                backgroundStyle: new(
                    color1: SettingsConverterV2.MergeColors(
                        color: previewStyle.ScreenStyle?.BackgroundStyle?.Color1,
                        defaultValue: defaultStyle.ScreenStyle?.BackgroundStyle?.Color1),
                    color2: SettingsConverterV2.MergeColors(
                        color: previewStyle.ScreenStyle?.BackgroundStyle?.Color2,
                        defaultValue: defaultStyle.ScreenStyle?.BackgroundStyle?.Color2)
                )
            ),
            extraColors: defaultStyle.ExtraColors);
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
