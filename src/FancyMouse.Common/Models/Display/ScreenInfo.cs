﻿using FancyMouse.Common.Models.Drawing;

namespace FancyMouse.Common.Models.Display;

/// <summary>
/// Immutable version of a System.Windows.Forms.Screen object so we don't need to
/// take a dependency on WinForms just for screen info.
/// </summary>
public sealed record ScreenInfo
{
    public ScreenInfo(int handle, bool primary, RectangleInfo displayArea, RectangleInfo workingArea)
    {
        // this.Handle is a HMONITOR that has been cast to an int because we don't want
        // to expose the HMONITOR type outside the current assembly.
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

    public RectangleInfo WorkingArea
    {
        get;
    }
}
