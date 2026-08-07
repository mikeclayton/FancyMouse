using System.Collections.ObjectModel;

namespace FancyMouse.Models.Display;

public sealed record DisplayInfo
{
    public DisplayInfo(IEnumerable<DeviceInfo> devices)
    {
        this.Devices = (devices ?? throw new ArgumentNullException(nameof(devices))).ToList().AsReadOnly();
    }

    public ReadOnlyCollection<DeviceInfo> Devices
    {
        get;
    }
}
