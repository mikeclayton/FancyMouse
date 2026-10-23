using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;

namespace FancyMouse.Editor;

/// <summary>
/// A hard-coded multi-monitor layout for trying the editor out on a machine with fewer real
/// screens - loaded instead of the real ones when the app is started with <c>--demo</c>.
/// </summary>
/// <remarks>
/// Laid out the way Display Settings would report it: the primary screen at the origin, every
/// other screen edge-to-edge with a neighbour (so they start out touching but not overlapping),
/// and a mix of resolutions, a portrait screen, and negative coordinates for screens above and
/// to the left of the primary one.
/// </remarks>
internal static class DemoScreens
{
    public static IReadOnlyList<ScreenInfo> Screens { get; } =
    [

        // primary - a large 4K screen, in the middle of the desk
        DemoScreens.Screen(primary: true, x: 0, y: 0, width: 3840, height: 2160),

        // a portrait screen to the left of the primary, sitting a little higher
        DemoScreens.Screen(primary: false, x: -1440, y: -400, width: 1440, height: 2560),

        // a smaller landscape screen to the right of the primary, sitting a little lower
        DemoScreens.Screen(primary: false, x: 3840, y: 360, width: 2560, height: 1440),

        // a laptop panel below the primary
        DemoScreens.Screen(primary: false, x: 960, y: 2160, width: 1920, height: 1200),

        // a small display below the portrait screen
        DemoScreens.Screen(primary: false, x: -1280, y: 2160, width: 1280, height: 800),
    ];

    private static ScreenInfo Screen(bool primary, int x, int y, int width, int height)
    {
        var area = new RectangleInfo(x, y, width, height);
        return new ScreenInfo(handle: 0, primary: primary, displayArea: area, workingArea: area);
    }
}
