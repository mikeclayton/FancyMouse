using System.Collections.ObjectModel;

using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Display;

/// <summary>
/// Represents a device whose screens are rendered in the preview image.
/// </summary>
public sealed record DeviceInfo
{
    public DeviceInfo(string hostname, bool localhost, IEnumerable<ScreenInfo> screens)
    {
        this.Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
        this.Localhost = localhost;
        this.Screens = (screens ?? throw new ArgumentNullException(nameof(screens))).ToList().AsReadOnly();
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

    /// <summary>
    /// Returns the screen that contains <paramref name="pt"/>, or null if the point is
    /// outside every screen (or the device has no screens).
    /// </summary>
    public ScreenInfo? GetScreenFromPoint(PointInfo pt)
    {
        ArgumentNullException.ThrowIfNull(pt);

        return this.Screens
            .SingleOrDefault(screen => screen.DisplayArea.Contains(pt));
    }
}
