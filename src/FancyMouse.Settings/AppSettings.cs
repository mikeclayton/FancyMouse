using System.Drawing;

using FancyMouse.HotKeys;
using FancyMouse.Models.Styles;
using FancyMouse.Settings.V2;

using Keys = FancyMouse.HotKeys.Keys;

namespace FancyMouse.Settings;

/// <summary>
/// Represents the settings used to control application behaviour.
/// This is different to the AppConfig class that is used to
/// serialize / deserialize settings into the application config file.
/// </summary>
/// <remarks>
/// This is a *settings* representation, not a *visual* one - <see cref="PreviewStyle"/> here
/// always carries the user's own custom style, in full, regardless of <see cref="PreviewType"/> -
/// it's what a settings UI would write back to config. Deriving what to actually render (copying
/// this and overlaying a built-in preset when <see cref="PreviewType"/> selects one) is a separate
/// step - see <see cref="V2.SettingsConverterV2.GetActivePreviewStyle"/> - performed at the point
/// of rendering, not baked in here, so this representation is never itself overwritten by a
/// preset's values.
/// </remarks>
public sealed class AppSettings
{
    public static readonly AppSettings DefaultSettings = new(
        hotkey: new(
            key: Keys.F,
            modifiers: KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift
        ),
        previewType: PreviewType.Bezelled,
        previewStyle: new(
            canvasSize: new(
                width: 1600,
                height: 1200
            ),
            canvasStyle: new(
                marginStyle: MarginStyle.Empty,
                borderStyle: new(
                    color: SystemColors.Highlight,
                    all: 8,
                    depth: 2
                ),
                paddingStyle: new(
                    all: 6
                ),
                backgroundStyle: new(
                    color1: Color.FromArgb(0xFF, 0x0D, 0x57, 0xD2),
                    color2: Color.FromArgb(0xFF, 0x03, 0x44, 0xC0)
                )
            ),
            screenStyle: new(
                marginStyle: new(
                    all: 4
                ),
                borderStyle: new(
                    color: Color.FromArgb(0xFF, 0x22, 0x22, 0x22),
                    all: 15,
                    depth: 3
                ),
                paddingStyle: PaddingStyle.Empty,
                backgroundStyle: new(
                    color1: Color.MidnightBlue,
                    color2: Color.MidnightBlue
                )
            ),
            extraColors: new[]
            {
                Color.Red,
                Color.Blue,
                Color.Green,
            }
        ),
        telemetryEnabled: false
    );

    public AppSettings(
        Keystroke hotkey,
        PreviewStyle previewStyle,
        PreviewType previewType,
        bool telemetryEnabled)
    {
        this.Hotkey = hotkey ?? throw new ArgumentNullException(nameof(hotkey));
        this.PreviewStyle = previewStyle ?? throw new ArgumentNullException(nameof(previewStyle));
        this.PreviewType = previewType;
        this.TelemetryEnabled = telemetryEnabled;
    }

    public Keystroke Hotkey
    {
        get;
    }

    /// <summary>
    /// Gets the user's own custom style, in full - see this class's own remarks for why this must
    /// never be a preset-overlaid value, regardless of <see cref="PreviewType"/>.
    /// </summary>
    public PreviewStyle PreviewStyle
    {
        get;
    }

    /// <summary>
    /// Gets which style should actually be rendered - one of the built-in presets, or
    /// <see cref="PreviewStyle"/> itself for <see cref="V2.PreviewType.Custom"/>. See
    /// <see cref="V2.SettingsConverterV2.GetActivePreviewStyle"/> for deriving the one to render.
    /// </summary>
    public PreviewType PreviewType
    {
        get;
    }

    /// <remarks>
    /// Gets a telemetry opt-in value, off by default.
    /// </remarks>
    public bool TelemetryEnabled
    {
        get;
    }
}
