using FancyMouse.Common.NativeMethods;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;

namespace FancyMouse.Common.Helpers;

public static class ScreenHelper
{
    public static ScreenInfo GetScreenFromPoint(
        List<ScreenInfo> screens,
        PointInfo pt)
    {
        // get the monitor handle from the point
        var hMonitor = User32.MonitorFromPoint(
            new((int)pt.X, (int)pt.Y),
            User32.MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        if (hMonitor.IsNull)
        {
            throw new InvalidOperationException($"no monitor found for point {pt}");
        }

        // find the screen with the given monitor handle
        var screen = screens
            .Single(item => item.Handle == hMonitor);
        return screen;
    }
}
