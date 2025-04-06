using FancyMouse.Common.Models.Display;
using FancyMouse.Common.Models.Drawing;

namespace FancyMouse.Common.Helpers;

public static class DeviceHelper
{
    public static ScreenInfo GetActivatedScreen(DisplayInfo displayInfo, PointInfo activatedLocation)
    {
        ArgumentNullException.ThrowIfNull(displayInfo);
        ArgumentNullException.ThrowIfNull(activatedLocation);

        var activatedScreen = displayInfo.Devices
            .Single(device => device.Localhost)
            .Screens
            .Single(screen => screen.DisplayArea.Contains(activatedLocation));

        return activatedScreen;
    }

    public static ScreenInfo GetActivatedScreen(DeviceInfo deviceInfo, PointInfo activatedLocation)
    {
        ArgumentNullException.ThrowIfNull(deviceInfo);
        ArgumentNullException.ThrowIfNull(activatedLocation);

        var activatedScreen = deviceInfo.Screens
            .Single(screen => screen.DisplayArea.Contains(activatedLocation));

        return activatedScreen;
    }
}
