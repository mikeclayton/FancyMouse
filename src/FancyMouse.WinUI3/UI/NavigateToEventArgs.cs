using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;

namespace FancyMouse.WinUI3.UI;

/// <summary>
/// Carries the intent behind a screen click, or a keyboard shortcut that means the same thing
/// (1-9, arrow keys, P, Home, End) - "move the pointer to this location on this device". Mouse
/// and keyboard both resolve to the exact same event so the host only needs one handler for
/// either.
/// </summary>
public sealed class NavigateToEventArgs : EventArgs
{
    public NavigateToEventArgs(DeviceInfo device, PointInfo location)
    {
        this.Device = device ?? throw new ArgumentNullException(nameof(device));
        this.Location = location ?? throw new ArgumentNullException(nameof(location));
    }

    /// <summary>
    /// Gets the device the target screen belongs to - lets the host distinguish a local device
    /// (move the cursor directly) from a remote one (route the move elsewhere) once remote
    /// devices are supported.
    /// </summary>
    public DeviceInfo Device
    {
        get;
    }

    /// <summary>
    /// Gets the physical desktop location the target screen maps onto - already resolved
    /// against that screen's own display area, ready to pass to
    /// <c>MouseHelper.SetCursorPosition</c>.
    /// </summary>
    public PointInfo Location
    {
        get;
    }
}
