using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FancyMouse.Common.Models.Display;
using FancyMouse.Common.Models.Drawing;

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

        // append a fake device just to see what it looks like
        devices.Add(
            new(
                hostname: "my_remote_machine",
                localhost: false,
                screens: new List<ScreenInfo>
                {
                    new(
                        handle: 0,
                        primary: true,
                        displayArea: new(-1920, -480, 1920, 1080),
                        workingArea: new(-1920, -480, 1920, 1080)),
                    new(
                        handle: 0,
                        primary: false,
                        displayArea: new(0, 0, 5120, 1440),
                        workingArea: new(0, 0, 5120, 1440)),
                }));

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
