using FancyMouse.Models.Drawing;

namespace FancyMouse.WinUI3.UI;

public sealed class ScreenshotClickedEventArgs : EventArgs
{
    public ScreenshotClickedEventArgs(PointInfo location)
    {
        this.Location = location ?? throw new ArgumentNullException(nameof(location));
    }

    /// <summary>
    /// Gets the physical desktop location the clicked screenshot maps onto - already resolved
    /// against the clicked screen's own display area, ready to pass to
    /// <c>MouseHelper.SetCursorPosition</c>.
    /// </summary>
    public PointInfo Location
    {
        get;
    }
}
