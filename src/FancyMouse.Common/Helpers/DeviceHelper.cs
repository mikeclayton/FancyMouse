using FancyMouse.Models.Display;

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
}
