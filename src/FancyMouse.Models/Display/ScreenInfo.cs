using System.Text.Json.Serialization;

using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Display;

/// <summary>
/// Immutable version of a System.Windows.Forms.Screen object so we don't need to
/// take a dependency on WinForms just for screen info.
/// </summary>
public sealed record ScreenInfo
{
    public ScreenInfo(int handle, bool primary, RectangleInfo displayArea, RectangleInfo? workingArea)
    {
        // this.Handle is a HMONITOR that has been cast to an int because we don't want
        // to expose the HMONITOR type outside the current assembly.
        this.Handle = handle;
        this.Primary = primary;
        this.DisplayArea = displayArea ?? throw new ArgumentNullException(nameof(displayArea));
        this.WorkingArea = workingArea;
    }

    [JsonPropertyName("handle")]
    public int Handle
    {
        get;
    }

    [JsonPropertyName("primary")]
    public bool Primary
    {
        get;
    }

    [JsonPropertyName("displayArea")]
    public RectangleInfo DisplayArea
    {
        get;
    }

    [JsonPropertyName("workingArea")]
    public RectangleInfo? WorkingArea
    {
        get;
    }
}
