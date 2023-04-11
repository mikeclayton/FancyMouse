﻿using System.Collections.ObjectModel;
using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Layout;

/// <summary>
/// Represents a collection of values needed for calculating the MainForm layout.
/// </summary>
public sealed class LayoutConfig
{
    public LayoutConfig(
        RectangleInfo virtualScreen,
        IEnumerable<RectangleInfo> screenBounds,
        PointInfo activatedLocation,
        int activatedScreenIndex,
        int activatedScreenNumber,
        SizeInfo maximumFormSize,
        PaddingInfo formPadding,
        PaddingInfo previewPadding)
    {
        // make sure the virtual screen entirely contains all of the individual screen bounds
        ArgumentNullException.ThrowIfNull(virtualScreen);
        ArgumentNullException.ThrowIfNull(screenBounds);
        if (screenBounds.Any(screen => !virtualScreen.Contains(screen)))
        {
            throw new ArgumentException($"'{nameof(virtualScreen)}' must contain all of the screens in '{nameof(screenBounds)}'", nameof(virtualScreen));
        }

        this.VirtualScreen = virtualScreen;
        this.ScreenBounds = new(screenBounds.ToList());
        this.ActivatedLocation = activatedLocation;
        this.ActivatedScreenIndex = activatedScreenIndex;
        this.ActivatedScreenNumber = activatedScreenNumber;
        this.MaximumFormSize = maximumFormSize;
        this.FormPadding = formPadding;
        this.PreviewPadding = previewPadding;
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
    /// Gets a collection containing the bounds of all of the individual screens connected to the system.
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
    /// constraints such as being too close to edge of a screen, in which case
    /// the form will be displayed centered as close as possible to this location.
    /// </summary>
    public PointInfo ActivatedLocation
    {
        get;
    }

    /// <summary>
    /// Gets the index of the screen the cursor was on when the form was activated.
    /// The value is an index into the ScreenBounds array and is 0-indexed as a result.
    /// </summary>
    public int ActivatedScreenIndex
    {
        get;
    }

    /// <summary>
    /// Gets the screen number the cursor was on when the form was activated.
    /// The value matches the screen numbering scheme in the "Display Settings" dialog
    /// and is 1-indexed as a result.
    /// </summary>
    public int ActivatedScreenNumber
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
