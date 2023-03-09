﻿using System.Collections.ObjectModel;

namespace FancyMouse.Drawing.Models;

internal sealed class LayoutConfig
{
    public LayoutConfig(
        Rectangle virtualScreen,
        IEnumerable<Rectangle> screenBounds,
        Point activatedLocation,
        int activatedScreen,
        Size maximumFormSize,
        Padding formPadding,
        Padding previewPadding)
    {
        this.VirtualScreen = new RectangleInfo(virtualScreen);
        this.ScreenBounds = new(
            (screenBounds ?? throw new ArgumentNullException(nameof(screenBounds)))
                .Select(screen => new RectangleInfo(screen))
                .ToList());
        this.ActivatedLocation = new(activatedLocation);
        this.ActivatedScreen = activatedScreen;
        this.MaximumFormSize = new(maximumFormSize);
        this.FormPadding = new(formPadding);
        this.PreviewPadding = new(previewPadding);
    }

    /// <summary>
    /// Gets the coordinates of the entire virtual screen.
    /// </summary>
    /// <remarks>
    /// The Virtual Screen is the bounding rectangle of all the monitors.
    /// https://learn.microsoft.com/en-us/windows/win32/gdi/the-virtual-screen
    /// </remarks>
    public RectangleInfo VirtualScreen
    {
        get;
    }

    /// <summary>
    /// Gets the bounds of all of the screens connected to the system.
    /// </summary>
    public ReadOnlyCollection<RectangleInfo> ScreenBounds
    {
        get;
    }

    /// <summary>
    /// Gets the point where the cursor was located when the form was activated.
    /// </summary>
    /// <summary>
    /// The preview form will be centered on this location unless there are any
    /// constraints such as the being too close to edge of a screen, in which case
    /// the form will be displayed as close as possible to this location.
    /// </summary>
    public PointInfo ActivatedLocation
    {
        get;
    }

    public int ActivatedScreen
    {
        get;
    }

    /// <summary>
    /// Gets the maximum size of the screen preview form.
    /// </summary>
    public SizeInfo MaximumFormSize
    {
        get;
    }

    /// <summary>
    /// Gets the padding border around the screen preview form.
    /// </summary>
    public PaddingInfo FormPadding
    {
        get;
    }

    /// <summary>
    /// Gets the padding border inside the screen preview image.
    /// </summary>
    public PaddingInfo PreviewPadding
    {
        get;
    }
}
