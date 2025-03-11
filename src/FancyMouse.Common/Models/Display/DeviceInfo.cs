using System.Collections.ObjectModel;
using FancyMouse.Common.Models.Drawing;

namespace FancyMouse.Common.Models.Display;

/// <summary>
/// Represents a device whose screens are rendered in the preview image.
/// </summary>
public sealed record DeviceInfo
{
    public DeviceInfo(string hostname, bool localhost, IEnumerable<ScreenInfo> screens)
    {
        this.Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
        this.Localhost = localhost;
        this.Screens = new(
            (screens ?? throw new ArgumentNullException(nameof(screens)))
                .ToList());
    }

    public string Hostname
    {
        get;
    }

    public bool Localhost
    {
        get;
    }

    public ReadOnlyCollection<ScreenInfo> Screens
    {
        get;
    }

    public RectangleInfo GetCombinedDisplayArea()
    {
        return RectangleInfo.Union(this.Screens.Select(screen => screen.DisplayArea));
    }
}
