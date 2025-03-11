using System.Collections.ObjectModel;

namespace FancyMouse.Common.Models.Display;

public sealed record DisplayInfo
{
    public DisplayInfo(IEnumerable<DeviceInfo> devices)
    {
        this.Devices = new(
            (devices ?? throw new ArgumentNullException(nameof(devices)))
                .ToList());
    }

    public ReadOnlyCollection<DeviceInfo> Devices
    {
        get;
    }
}
