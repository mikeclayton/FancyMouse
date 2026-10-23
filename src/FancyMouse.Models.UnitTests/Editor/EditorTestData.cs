using System.Drawing;

using FancyMouse.Models.Display;
using FancyMouse.Models.Styles;

namespace FancyMouse.Models.UnitTests.Editor;

/// <summary>
/// A small, round-numbered preview style and desktop so expected placements can be worked out by
/// hand:
/// <para/>
/// * the preview is 1000 x 800 with a 10px border and 10px padding, so its content area is
///   (20, 20, 960, 760)
/// * each screen has a 2px margin and 5px border, so 14px of "chrome" on each axis
/// * the desktop is two 1000 x 500 screens side by side (2000 x 500), which scales by 0.48 to fit
///   the content width - 960 x 240, centred vertically at y = 20 + ((760 - 240) / 2) = 280
/// </summary>
internal static class EditorTestData
{
    public const double Delta = 1e-9;

    public static PreviewStyle PreviewStyle { get; } = new(
        canvasSize: new(1000, 800),
        canvasStyle: new(
            marginStyle: MarginStyle.Empty,
            borderStyle: new(Color.Blue, 10, 0),
            paddingStyle: new(10),
            backgroundStyle: BackgroundStyle.Empty),
        screenStyle: new(
            marginStyle: new(2),
            borderStyle: new(Color.Black, 5, 0),
            paddingStyle: PaddingStyle.Empty,
            backgroundStyle: BackgroundStyle.Empty),
        extraColors: []);

    public static IReadOnlyList<ScreenInfo> Screens { get; } =
    [
        new(handle: 0, primary: true, displayArea: new(0, 0, 1000, 500), workingArea: null),
        new(handle: 0, primary: false, displayArea: new(1000, 0, 1000, 500), workingArea: null),
    ];
}
