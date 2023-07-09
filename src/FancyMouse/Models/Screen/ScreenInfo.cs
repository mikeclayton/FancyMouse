using FancyMouse.Models.Drawing;
using FancyMouse.NativeMethods;

namespace FancyMouse.Models.Screen;

/// <summary>
/// Immutable version of a System.Windows.Forms.Screen object so we don't need to
/// take a dependency on WinForms just for screen info.
/// </summary>
public sealed class ScreenInfo
{
    internal ScreenInfo(Core.HMONITOR handle, bool primary, RectangleInfo displayArea, RectangleInfo workingArea)
    {
        this.Handle = handle;
        this.Primary = primary;
        this.DisplayArea = displayArea ?? throw new ArgumentNullException(nameof(displayArea));
        this.WorkingArea = workingArea ?? throw new ArgumentNullException(nameof(workingArea));
    }

    public int Handle
    {
        get;
    }

    public bool Primary
    {
        get;
    }

    public RectangleInfo DisplayArea
    {
        get;
    }

    public RectangleInfo Bounds =>
        this.DisplayArea;

    public RectangleInfo WorkingArea
    {
        get;
    }
}
