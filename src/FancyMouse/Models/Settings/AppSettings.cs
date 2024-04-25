using FancyMouse.Common.Models.Styles;
using FancyMouse.HotKeys;
using Keys = FancyMouse.HotKeys.Keys;

namespace FancyMouse.Models.Settings;

/// <summary>
/// Represents the settings used to control application behaviour.
/// This is different to the AppConfig class that is used to
/// serialize / deserialize settings into the application config file.
/// </summary>
internal sealed class AppSettings
{
    public static readonly AppSettings DefaultSettings = new(
        hotkey: new(
            key: Keys.F,
            modifiers: KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift
        ),
        previewStyle: new(
            canvasSize: new(
                width: 1600,
                height: 1200
            ),
            canvasStyle: new(
                marginStyle: MarginStyle.Empty,
                borderStyle: new(
                    color: SystemColors.Highlight,
                    all: 6,
                    depth: 0
                ),
                paddingStyle: new(
                    all: 4
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
                    all: 12,
                    depth: 4
                ),
                paddingStyle: PaddingStyle.Empty,
                backgroundStyle: new(
                    color1: Color.MidnightBlue,
                    color2: Color.MidnightBlue
                )
            )
        )
    );

    public AppSettings(
        Keystroke hotkey,
        PreviewStyle previewStyle)
    {
        this.Hotkey = hotkey ?? throw new ArgumentNullException(nameof(hotkey));
        this.PreviewStyle = previewStyle ?? throw new ArgumentNullException(nameof(previewStyle));
    }

    public Keystroke Hotkey
    {
        get;
    }

    public PreviewStyle PreviewStyle
    {
        get;
    }
}
