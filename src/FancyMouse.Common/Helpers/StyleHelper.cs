using System.Drawing;

using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;

namespace FancyMouse.Common.Helpers;

public static class StyleHelper
{
    /// <summary>
    /// Compact (legacy) preview style
    /// </summary>
    public static readonly PreviewStyle CompactPreviewStyle = new(
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
                all: 0
            ),
            backgroundStyle: new(
                color1: Color.FromArgb(0xFF, 0x0D, 0x57, 0xD2),
                color2: Color.FromArgb(0xFF, 0x03, 0x44, 0xC0)
            )
        ),
        screenStyle: new(
            marginStyle: new(
                all: 0
            ),
            borderStyle: new(
                color: Color.FromArgb(0xFF, 0x22, 0x22, 0x22),
                all: 0,
                depth: 0
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
    );

    /// <summary>
    /// Bezelled preview style
    /// </summary>
    public static readonly PreviewStyle BezelledPreviewStyle = new(
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
    );

    public static PreviewStyle WithCanvasSize(this PreviewStyle previewStyle, SizeInfo canvasSize)
    {
        ArgumentNullException.ThrowIfNull(previewStyle);
        ArgumentNullException.ThrowIfNull(canvasSize);
        return new PreviewStyle(
            canvasSize: canvasSize,
            canvasStyle: previewStyle.CanvasStyle,
            screenStyle: previewStyle.ScreenStyle,
            extraColors: previewStyle.ExtraColors);
    }

    /// <summary>
    /// The equivalent of PowerToys' own <c>SettingsHelper.GetActivePreviewStyle</c> - derives the
    /// *visual* representation to render from a *settings* representation
    /// (<paramref name="previewStyle"/>/<paramref name="previewType"/>, e.g. FancyMouse's own
    /// <c>AppSettings.PreviewStyle</c>/<c>AppSettings.PreviewType</c>): copies
    /// <paramref name="previewStyle"/>'s canvas size onto whichever of
    /// <see cref="CompactPreviewStyle"/>/<see cref="BezelledPreviewStyle"/> is selected, or returns
    /// <paramref name="previewStyle"/> itself unchanged for Custom. Never mutates
    /// <paramref name="previewStyle"/> - call this at the point of rendering, not when
    /// parsing/loading settings, so the settings representation it derives from is never itself
    /// overwritten by a preset's values.
    /// </summary>
    /// <remarks>
    /// Lives here, next to the presets it chooses between, rather than with any one app's settings
    /// format - it only takes the shared <see cref="PreviewStyle"/>/<see cref="PreviewType"/> types,
    /// so each app's settings code resolves to those and calls this, without the settings code
    /// itself needing to depend on this assembly.
    /// </remarks>
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
}
