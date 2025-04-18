using System.Collections.ObjectModel;

using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Display;

/// <summary>
/// Represents the entire desktop or virtual screen for a single logical device.
/// </summary>
public sealed record DesktopInfo
{
    public DesktopInfo(IEnumerable<ScreenInfo> screens)
    {
        this.Screens = new(
            (screens ?? throw new ArgumentNullException(nameof(screens)))
                .ToList());
    }

    public ReadOnlyCollection<ScreenInfo> Screens
    {
        get;
    }

    public RectangleInfo GetCombinedDisplayArea()
    {
        return (this.Screens.Count == 0)
            ? throw new InvalidOperationException($"{nameof(GetCombinedDisplayArea)} requires one or more screens.")
            : RectangleInfo.Union(this.Screens.Select(screen => screen.DisplayArea));
    }
}
