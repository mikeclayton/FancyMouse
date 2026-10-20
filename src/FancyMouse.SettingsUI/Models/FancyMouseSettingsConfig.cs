using FancyMouse.Models.Styles;
using FancyMouse.Settings.V2;

namespace FancyMouse.SettingsUI.Models;

/// <summary>
/// Equivalent to PowerToys' <c>MouseJumpSettings</c>
/// </summary>
public sealed class FancyMouseSettingsConfig
{
    public FancyMouseSettingsConfig(AppConfig appConfig, FancyMouseProperties properties)
    {
        this.AppConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        this.Properties = properties ?? throw new ArgumentNullException(nameof(properties));
    }

    /// <summary>
    /// Gets the settings file as it was loaded.
    /// </summary>
    /// <remarks>
    /// <see cref="Properties"/> only covers the settings this UI edits, so this carries the
    /// rest (e.g. the version and telemetry settings) through to each save unchanged.
    /// </remarks>
    internal AppConfig AppConfig
    {
        get;
    }

    public FancyMouseProperties Properties
    {
        get;
    }

    /// <summary>
    /// Combines the specified parameters into a new FancyMouseSettingsConfig instance.
    /// Custom style settings are preserved regardless of PreviewType so that the
    /// settings can be used if the preview type is changed to "custom".
    /// </summary>
    public static FancyMouseSettingsConfig FromPreviewStyle(AppConfig appConfig, PreviewStyle customStyle, string? hotkey, string? previewType)
    {
        ArgumentNullException.ThrowIfNull(appConfig);
        ArgumentNullException.ThrowIfNull(customStyle);

        var canvasStyle = customStyle.CanvasStyle;
        var screenStyle = customStyle.ScreenStyle;

        var properties = new FancyMouseProperties
        {
            ActivationShortcut = hotkey,
            PreviewType = previewType,
            ThumbnailSize = new FancyMouseThumbnailSize
            {
                Width = (double)customStyle.CanvasSize.Width,
                Height = (double)customStyle.CanvasSize.Height,
            },
            BackgroundColor1 = FancyMouseSettingsConfig.ToHexString(canvasStyle.BackgroundStyle.Color1),
            BackgroundColor2 = FancyMouseSettingsConfig.ToHexString(canvasStyle.BackgroundStyle.Color2),
            BorderColor = FancyMouseSettingsConfig.ToHexString(canvasStyle.BorderStyle.Color),
            BorderThickness = (double)canvasStyle.BorderStyle.Top,
            Border3dDepth = (double)canvasStyle.BorderStyle.Depth,
            BorderPadding = (double)canvasStyle.PaddingStyle.Top,
            BezelColor = FancyMouseSettingsConfig.ToHexString(screenStyle.BorderStyle.Color),
            BezelThickness = (double)screenStyle.BorderStyle.Top,
            Bezel3dDepth = (double)screenStyle.BorderStyle.Depth,
            ScreenMargin = (double)screenStyle.MarginStyle.Top,
            ScreenColor1 = FancyMouseSettingsConfig.ToHexString(screenStyle.BackgroundStyle.Color1),
            ScreenColor2 = FancyMouseSettingsConfig.ToHexString(screenStyle.BackgroundStyle.Color2),
        };

        return new FancyMouseSettingsConfig(appConfig, properties);
    }

    private static string? ToHexString(System.Drawing.Color? color)
        => color.HasValue ? $"#{color.Value.R:X2}{color.Value.G:X2}{color.Value.B:X2}" : null;
}
