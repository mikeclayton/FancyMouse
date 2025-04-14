using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
