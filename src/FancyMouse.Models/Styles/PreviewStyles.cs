using System.Drawing;

namespace FancyMouse.Models.Styles;

/// <summary>
/// The predefined preview styles - the shared data behind the "Compact" and "Bezelled"
/// <see cref="PreviewType"/> options, in the same spirit as <see cref="System.Drawing.Brushes"/>
/// or <see cref="System.Drawing.Pens"/>.
/// </summary>
public static class PreviewStyles
{
    /// <summary>
    /// Compact (legacy) preview style
    /// </summary>
    public static readonly PreviewStyle Compact = new(
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
    public static readonly PreviewStyle Bezelled = new(
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
}
