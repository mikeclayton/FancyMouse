using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;

namespace FancyMouse.Common.Helpers;

public static class DeviceHelper
{
    public static DisplayInfo GetDisplayInfo()
    {
        var devices = new List<DeviceInfo>();

        // add the local devices
        devices.Add(
            new(
                hostname: Environment.MachineName,
                localhost: true,
                screens: ScreenHelper.GetAllScreens()));

        return new DisplayInfo(
            devices: devices);
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
