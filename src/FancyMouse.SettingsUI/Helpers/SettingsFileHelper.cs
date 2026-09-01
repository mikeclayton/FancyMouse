using System.Text.Json;

using FancyMouse.Common.Helpers;
using FancyMouse.Models.Styles;
using FancyMouse.Settings;
using FancyMouse.Settings.V2;
using FancyMouse.SettingsUI.Models;

namespace FancyMouse.SettingsUI.Helpers;

/// <summary>
/// Loads and saves the same <c>appSettings.json</c> file <c>FancyMouse.WinUI3</c> reads - that
/// app already has a <c>FileSystemWatcher</c> on it (see its own <c>ConfigHelper</c>) that
/// reloads settings live whenever the file changes on disk, so this tool doesn't need any IPC of
/// its own to reach a running instance - saving the file is enough.
/// </summary>
internal static class SettingsFileHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private static readonly SerializationContextV2 JsonSerializationContext = new(SettingsFileHelper.JsonSerializerOptions);

    /// <summary>
    /// Reads the config file and returns two representations:
    /// * an AppConfig that contains a high-fidelity representation of the raw config file,
    ///   including nulls for missing properties and preserving custom style settings regardless
    ///   of the actual style in effect (e.g. "compact", "bezelled", "custom").
    /// * a FancyMouseSettingsConfig - a representation of the AppConfig instance with default
    ///   values applied for missing style properties.
    /// </summary>
    public static (AppConfig Config, FancyMouseSettingsConfig SettingsConfig) Load(string path)
    {
        var json = File.Exists(path) ? File.ReadAllText(path) : null;
        var appConfig = (json is not null)
            ? JsonSerializer.Deserialize<AppConfig>(json, SettingsFileHelper.JsonSerializationContext.AppConfig)
                ?? throw new InvalidOperationException($"Failed to parse '{path}'.")
            : new AppConfig(version: 2, hotkey: null, preview: null, telemetry: null);

        var previewStyle = SettingsConverterV2.MergePreviewStyles(appConfig.Preview, AppSettings.DefaultSettings.PreviewStyle);
        var settingsConfig = FancyMouseSettingsConfig.FromPreviewStyle(previewStyle, appConfig.Hotkey, appConfig.Preview?.Type);
        return (appConfig, settingsConfig);
    }

    /// <summary>
    /// Combines a set of configuration values (original config, new preview styles,
    /// and hotkey / preview type overrides) and writes the result to the config file.
    /// </summary>
    /// <param name="customStyle">
    /// The "custom" style settings to write to the config file regardless of which
    /// preview type is active - this allows the "custom" settings to be preserved
    /// and restored if the preview type is changed to "custom".
    /// </param>
    public static void Save(string path, AppConfig original, PreviewStyle customStyle, string? hotkey, string? previewType)
    {
        var updated = new AppConfig(
            version: original.Version,
            hotkey: hotkey,
            preview: SettingsFileHelper.ToPreviewStyleSettings(customStyle, previewType),
            telemetry: original.Telemetry);

        var json = JsonSerializer.Serialize(updated, SettingsFileHelper.JsonSerializationContext.AppConfig);
        File.WriteAllText(path, json);
    }

    private static PreviewStyleSettings ToPreviewStyleSettings(PreviewStyle previewStyle, string? previewType)
    {
        var canvasStyle = previewStyle.CanvasStyle;
        var screenStyle = previewStyle.ScreenStyle;

        return new PreviewStyleSettings(
            type: previewType,
            canvasSize: new CanvasSizeSettings(
                width: (int)previewStyle.CanvasSize.Width,
                height: (int)previewStyle.CanvasSize.Height),
            canvasStyle: new CanvasStyleSettings(
                borderStyle: new BorderStyleSettings(
                    color: ColorHelper.SerializeToConfigColorString(
                        ColorHelper.ToUnnamedColor(canvasStyle.BorderStyle.Color)),
                    width: canvasStyle.BorderStyle.Top,
                    depth: canvasStyle.BorderStyle.Depth),
                paddingStyle: new PaddingStyleSettings((int)canvasStyle.PaddingStyle.Top),
                backgroundStyle: new BackgroundStyleSettings(
                    color1: ColorHelper.SerializeToConfigColorString(
                        ColorHelper.ToUnnamedColor(canvasStyle.BackgroundStyle.Color1)),
                    color2: ColorHelper.SerializeToConfigColorString(
                        ColorHelper.ToUnnamedColor(canvasStyle.BackgroundStyle.Color2)))),
            screenStyle: new ScreenStyleSettings(
                marginStyle: new MarginStyleSettings((int)screenStyle.MarginStyle.Top),
                borderStyle: new BorderStyleSettings(
                    color: ColorHelper.SerializeToConfigColorString(
                        ColorHelper.ToUnnamedColor(screenStyle.BorderStyle.Color)),
                    width: screenStyle.BorderStyle.Top,
                    depth: screenStyle.BorderStyle.Depth),
                backgroundStyle: new BackgroundStyleSettings(
                    color1: ColorHelper.SerializeToConfigColorString(
                        ColorHelper.ToUnnamedColor(screenStyle.BackgroundStyle.Color1)),
                    color2: ColorHelper.SerializeToConfigColorString(
                        ColorHelper.ToUnnamedColor(screenStyle.BackgroundStyle.Color2)))));
    }
}
