using System.Diagnostics;
using System.Drawing;

using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;

namespace FancyMouse.Drawing.Helpers;

public static class LayoutHelper
{
    /*

        we use a poor-man's version of the w3c "box model" (see FancyMouse.Models.Styles.BoxStyle)
        to calculate sizes and positions of individual ui components. the diagram below shows the
        logical organisation of components:

        * the overall PreviewWindow - positions itself on the desktop and directly hosts a
          child PreviewPane, sized to match it exactly. the whole canvas style belongs to the
          pane, including the outer *margin* (transparent space around the outer border), so
          the only thing the window has to coordinate with the pane is clipping itself to the
          pane's outer border to give it rounded corners (see PreviewWindow)

          +----------[preview window]----------+
          |+----------[preview pane]----------+|
          ||░░░░░░░░░░░░░[margin]░░░░░░░░░░░░░||
          ||░░+---------[see below]--------+░░||
          ||░░|                            |░░||
          ||░░|                            |░░||
          ||░░+----------------------------+░░||
          ||░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░||
          |+----------------------------------+|
          +------------------------------------+

        * the child PreviewPane - this contains the outer margin, the outer border, the preview
          background, the padding inside the outer border, the screen bezels and screenshot
          images. the diagram below starts at the outer border - the margin, if there is one,
          is transparent space around it within the pane's own bounds. the "device grid" in the
          diagram is a logical grid - it doesn't exist in the layout or control hierarchy, and
          only exists as a set of temporary bounds during the actual layout calculation.

          +-----------------------------[preview pane]------------------------------+
          |█████████████████████████████[outer border]██████████████████████████████|
          |██+---------------------------[background]----------------------------+██|
          |██|░░░░░░░░░░░░░░░░░░░░░░░░░░░░░[padding]░░░░░░░░░░░░░░░░░░░░░░░░░░░░░|██|
          |██|▒▒+------------------------[device grid]-+----------------------+▒▒|██|
          |██|▒▒|                                      |▓▓▓▓▓▓[device 2]▓▓▓▓▓▓|▒▒|██|
          |██|▒▒|                                      |▓▓░░░░[screen 1]░░░░▓▓|▒▒|██|
          |██|▒▒|▓▓▓▓▓▓▓▓▓▓▓▓▓▓[device 1]▓▓▓▓▓▓▓▓▓▓▓▓▓▓|▓▓░░              ░░▓▓|▒▒|██|
          |██|▒▒|▓▓░░░░[screen 1]░░░░░░[screen 2]░░░░▓▓|▓▓░░              ░░▓▓|▒▒|██|
          |██|▒▒|▓▓░░              ░░              ░░▓▓|▓▓░░              ░░▓▓|▒▒|██|
          |██|▒▒|▓▓░░              ░░              ░░▓▓|▓▓░░░░░░░░░░░░░░░░░░▓▓|▒▒|██|
          |██|▒▒|▓▓░░              ░░              ░░▓▓|▓▓░░░░[screen 2]░░░░▓▓|▒▒|██|
          |██|▒▒|▓▓░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓▓|▓▓░░              ░░▓▓|▒▒|██|
          |██|▒▒|▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓|▓▓░░              ░░▓▓|▒▒|██|
          |██|▒▒|                                      |▓▓░░              ░░▓▓|▒▒|██|
          |██|▒▒|                                      |▓▓░░░░░░░░░░░░░░░░░░▓▓|▒▒|██|
          |██|▒▒|                                      |▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓|▒▒|██|
          |██|▒▒+--------------------------------------+----------------------+▒▒|██|
          |██|▒▒|        ▓▓▓▓▓▓[device 3]▓▓▓▓▓▓        |                      |▒▒|██|
          |██|▒▒|        ▓▓░░░░[screen 1]░░░░▓▓        |                      |▒▒|██|
          |██|▒▒|        ▓▓░░              ░░▓▓        |                      |▒▒|██|
          |██|▒▒|        ▓▓░░              ░░▓▓        |   ▓▓▓[device 4]▓▓▓   |▒▒|██|
          |██|▒▒|        ▓▓░░              ░░▓▓        |   ▓▓░[screen 1]░▓▓   |▒▒|██|
          |██|▒▒|        ▓▓░░░░░░░░░░░░░░░░░░▓▓        |   ▓▓░░        ░░▓▓   |▒▒|██|
          |██|▒▒|        ▓▓░░░░[screen 2]░░░░▓▓        |   ▓▓░░        ░░▓▓   |▒▒|██|
          |██|▒▒|        ▓▓░░              ░░▓▓        |   ▓▓░░░░░░░░░░░░▓▓   |▒▒|██|
          |██|▒▒|        ▓▓░░              ░░▓▓        |   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓   |▒▒|██|
          |██|▒▒|        ▓▓░░              ░░▓▓        |                      |▒▒|██|
          |██|▒▒|        ▓▓░░░░░░░░░░░░░░░░░░▓▓        |                      |▒▒|██|
          |██|▒▒|        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓        |                      |▒▒|██|
          |██|▒▒+-------------------------------------------------------------+▒▒|██|
          |██|▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒|██|
          |██+-------------------------------------------------------------------+██|
          |█████████████████████████████████████████████████████████████████████████|
          +-------------------------------------------------------------------------+

        note that the canvas style settings in the config file map entirely onto the
        PreviewPane - its *margin*, *border*, *padding* and *background* - so any host renders
        the same preview from the same style. a host that doesn't want an outer border at all
        just sets the border thickness and padding to zero. (the canvas *margin* is part of the
        preview's own style, not the control's xaml Margin - a host can still set the latter
        separately if it wants extra space around the whole preview.)

    */

    /// <param name="maximumSize">
    /// An optional upper bound on the layout's size.
    /// If specified, device and screen dimensions will be scaled down as required in order for the final layout to fit inside the bounds.
    /// If <see langword="null"/>, the layout size will be calculated to allow full-size device and screen visualisations.
    /// </param>
    public static PreviewLayout GetPreviewLayout(
        PreviewStyle previewStyle, DisplayInfo displayInfo, SizeInfo? maximumSize = null)
    {
        ArgumentNullException.ThrowIfNull(previewStyle);
        ArgumentNullException.ThrowIfNull(displayInfo);

        // arrange the preview, canvas, devices and screens
        var previewLayout = LayoutHelper.CreateInitialPreviewLayout(previewStyle, maximumSize);
        LayoutHelper.AddDeviceAndScreenStyles(previewLayout, previewStyle, displayInfo);
        LayoutHelper.ArrangeAndScaleDeviceLayouts(previewLayout);
        LayoutHelper.ArrangeAndScaleScreenLayouts(previewLayout);
        LayoutHelper.ArrangeAndResizeCanvasLayout(previewLayout);
        previewLayout.PreviewSize = previewLayout.CanvasLayout?.CanvasBounds?.OuterBounds.Size
            ?? throw new InvalidOperationException();

        return previewLayout.Build();
    }

    /// <summary>
    /// Positions an arbitrary rectangle (typically a PreviewWindow's own outer bounds) on
    /// the desktop - centered on the activated location.
    /// </summary>
    public static RectangleInfo PositionOnScreen(RectangleInfo bounds, ScreenInfo activatedScreen, PointInfo activatedLocation)
    {
        ArgumentNullException.ThrowIfNull(bounds);
        ArgumentNullException.ThrowIfNull(activatedScreen);
        ArgumentNullException.ThrowIfNull(activatedLocation);

        // center the bounds on the activated location, *but* if the activated location is
        // near the edge of the screen that *could* cause the bounding box to fall partially
        // outside the screen bounds so we'll nudge it back inside the screen bounds as well
        return bounds
            .Center(activatedLocation)
            .MoveInside(activatedScreen.DisplayArea);
    }

    /// <summary>
    /// Works out the maximum pane size for the given constraints, and returns a
    /// <see cref="PreviewLayout.Builder"/> seeded with just <see cref="PreviewLayout.Builder.PreviewSize"/>
    /// and the canvas's own bounds/style - no devices yet, see <see cref="AddDeviceAndScreenStyles"/>.
    /// The canvas uses the whole canvas style as-is, so the preview pane renders its own margin
    /// and outer border, and its padding and background inside it, without any help from its host.
    /// </summary>
    private static PreviewLayout.Builder CreateInitialPreviewLayout(
        PreviewStyle previewStyle, SizeInfo? maximumSize = null)
    {
        ArgumentNullException.ThrowIfNull(previewStyle);

        // work out the maximum allowed size of the preview pane's footprint (margin included) -
        // pure size arithmetic, since PreviewLayout deliberately has no position, only a size
        // (see PreviewLayout):
        // * can't be bigger than maximumSize, if the caller gave one (e.g. the activated screen,
        //   for a caller rendering a real on-screen window)
        // * can't be bigger than the configured canvas size
        var paneMaxSize = (maximumSize is not null)
            ? previewStyle.CanvasSize.Clamp(maximumSize)
            : previewStyle.CanvasSize;
        var paneMaxBounds = new RectangleInfo(paneMaxSize);

        // this is the root of a nested structure of mutable "builder" objects that can be used
        // to build the final immutable layout objects once all the bounds have been calculated
        return new PreviewLayout.Builder
        {
            PreviewSize = SizeInfo.Empty,
            CanvasLayout = new()
            {
                CanvasBounds = BoxBounds.CreateFromOuterBounds(
                    outerBounds: paneMaxBounds,
                    boxStyle: previewStyle.CanvasStyle),
                CanvasStyle = previewStyle.CanvasStyle,
            },
        };
    }

    /// <summary>
    /// Populates <paramref name="previewLayout"/>'s device/screen layouts - one
    /// <see cref="DeviceLayout.Builder"/> per <paramref name="displayInfo"/> device, each with
    /// its screens' styles already resolved (see its own local <c>GetDeviceScreenColor</c>) but their
    /// bounds still <see cref="BoxBounds.Empty"/> placeholders, ready for
    /// <see cref="ArrangeAndScaleDeviceLayouts"/>/<see cref="ArrangeAndScaleScreenLayouts"/> to
    /// arrange for real.
    /// </summary>
    private static void AddDeviceAndScreenStyles(PreviewLayout.Builder previewLayout, PreviewStyle previewStyle, DisplayInfo displayInfo)
    {
        ArgumentNullException.ThrowIfNull(previewLayout);
        ArgumentNullException.ThrowIfNull(previewStyle);
        ArgumentNullException.ThrowIfNull(displayInfo);

        // check we have at least one device
        if (displayInfo.Devices.Count == 0)
        {
            throw new ArgumentException("Value must contain at least one device.", nameof(displayInfo));
        }

        var canvasLayout = previewLayout.CanvasLayout
            ?? throw new InvalidOperationException();

        canvasLayout.DeviceLayouts = displayInfo.Devices.Select(
            (deviceInfo, deviceIndex) =>
            {
                // the first device (index 0, assumed to be "localhost") gets the configured screen
                // border colour; every other device cycles through ExtraColors (repeating from
                // the start once exhausted), falling back to the configured colour if none are
                // configured.
                var borderColor = (deviceIndex == 0 || previewStyle.ExtraColors.Count == 0)
                    ? previewStyle.ScreenStyle.BorderStyle.Color
                    : previewStyle.ExtraColors[(deviceIndex - 1) % previewStyle.ExtraColors.Count];

                // only create a new ScreenStyle if the border color is different from the
                // configured one - this preserves a reference to the configured ScreenStyle
                // (and its own "ScreenStyle.IsEmpty == true" if the color is Transparent)
                // rather than always allocating a fresh, effectively-identical copy.
                var screenStyle = previewStyle.ScreenStyle.BorderStyle.Color == borderColor
                    ? previewStyle.ScreenStyle
                    : new BoxStyle(
                        previewStyle.ScreenStyle.MarginStyle,
                        previewStyle.ScreenStyle.BorderStyle.WithColor(borderColor),
                        previewStyle.ScreenStyle.PaddingStyle,
                        previewStyle.ScreenStyle.BackgroundStyle);

                return new DeviceLayout.Builder
                {
                    DeviceInfo = deviceInfo,
                    DeviceBounds = BoxBounds.Empty,
                    DeviceStyle = BoxStyle.Empty,
                    ScreenLayouts = deviceInfo.Screens.Select(
                        screenInfo => new ScreenLayout.Builder
                        {
                            ScreenInfo = screenInfo,
                            ScreenBounds = BoxBounds.Empty,
                            ScreenStyle = screenStyle,
                        }).ToList(),
                };
            }).ToList();
    }

    /// <summary>
    /// Arranges the device layouts into a non-overlapping grid and scales them
    /// so the grid fits inside the specified content bounds.
    /// </summary>
    private static void ArrangeAndScaleDeviceLayouts(PreviewLayout.Builder previewLayout)
    {
        var deviceLayouts = previewLayout.CanvasLayout?.DeviceLayouts
            ?? throw new InvalidOperationException();
        var contentBounds = previewLayout.CanvasLayout.CanvasBounds?.ContentBounds
            ?? throw new InvalidOperationException();

        // build an initial grid of devices at 100% scale. the grid is currently a single row
        // of cells high with all the devices arranged left to right for the time being.
        // we'll enhance this to allow for multiple rows later to support the
        // Mouse Without Borders "square" arrangement.
        var gridRowCount = 1;
        var gridColumnCount = deviceLayouts.Count;
        var deviceGrid = new DeviceLayout.Builder[gridRowCount, gridColumnCount];
        for (var columnIndex = 0; columnIndex < gridColumnCount; columnIndex++)
        {
            deviceGrid[0, columnIndex] = deviceLayouts[columnIndex];
        }

        // if a device has zero screens (or if we failed to connect and read screen layouts) we
        // still want to allocate space in the full size grid so that it doesn't disappear into
        // a zero width or height cell.
        var fullSizeGridCellMinSize = new Size(1024, 768);

        // find the size of the tallest device in each row and the widest device in each column.
        // this tells us how tall each row needs to be and how wide each column needs to be.
        var fullSizeGridCellBounds = new RectangleInfo[gridRowCount, gridColumnCount];
        var fullSizeRowHeights = new decimal[gridRowCount];
        var fullSizeColumnWidths = new decimal[gridColumnCount];
        for (var rowIndex = 0; rowIndex < gridRowCount; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < gridColumnCount; columnIndex++)
            {
                var deviceInfo = deviceGrid[rowIndex, columnIndex].DeviceInfo ?? throw new InvalidOperationException();
                var deviceBounds = deviceInfo.GetCombinedDisplayArea();

                // if the device bounds are empty, it probably means there aren't any
                // screens available for this device so we'll use the minimum size instead
                // for the grid cell size
                var fullSizeGridCell = deviceBounds.IsEmpty
                    ? deviceBounds.Resize(
                        width: fullSizeGridCellMinSize.Width,
                        height: fullSizeGridCellMinSize.Height)
                    : deviceBounds;

                fullSizeGridCellBounds[rowIndex, columnIndex] = fullSizeGridCell;
                fullSizeRowHeights[rowIndex] = Math.Max(fullSizeRowHeights[rowIndex], fullSizeGridCell.Height);
                fullSizeColumnWidths[columnIndex] = Math.Max(fullSizeColumnWidths[columnIndex], fullSizeGridCell.Width);
            }
        }

        // use the row heights to calculate the absolute coordinates of each grid row
        var fullSizeRowCoordinates = new decimal[gridRowCount];
        fullSizeRowCoordinates[0] = 0;
        for (var rowIndex = 1; rowIndex < gridRowCount; rowIndex++)
        {
            fullSizeRowCoordinates[rowIndex] = fullSizeRowCoordinates[rowIndex - 1] + fullSizeRowHeights[rowIndex - 1];
        }

        // use the column widths to calculate the absolute coordinates of each grid column
        var fullSizeColumnCoordinates = new decimal[gridColumnCount];
        fullSizeColumnCoordinates[0] = 0;
        for (var columnIndex = 1; columnIndex < gridColumnCount; columnIndex++)
        {
            fullSizeColumnCoordinates[columnIndex] = fullSizeColumnCoordinates[columnIndex - 1] + fullSizeColumnWidths[columnIndex - 1];
        }

        // calculate the full size bounds for the entire grid
        var fullSizeGridBounds = new RectangleInfo(
            x: 0,
            y: 0,
            width: fullSizeColumnCoordinates[gridColumnCount - 1] + fullSizeColumnWidths[gridColumnCount - 1],
            height: fullSizeRowCoordinates[gridRowCount - 1] + fullSizeRowHeights[gridRowCount - 1]);

        // work out the scaling factor that will fit the full size grid into the content bounds
        var scaledGridSize = fullSizeGridBounds.Size
            .ScaleToFit(contentBounds.Size, out var scalingFactor);

        // scale the row and column coordinates by the factor. round coordinates to ensure they
        // don't overlap when rendered as pixel dimensions in the image. we'll also add a final
        // "fencepost" coordinate to simplify building the end cell of each row and column in the
        // scaled grid.
        var scaledRowCoordinates = fullSizeRowCoordinates
            .Select(fullSizeRowCoordinate => Math.Round(fullSizeRowCoordinate * scalingFactor))
            .Concat([scaledGridSize.Height])
            .Select(scaledRowCoordinate => scaledRowCoordinate + contentBounds.Top)
            .ToArray();
        var scaledColumnCoordinates = fullSizeColumnCoordinates
            .Select(fullSizeColumnCoordinate => Math.Round(fullSizeColumnCoordinate * scalingFactor))
            .Concat([scaledGridSize.Width])
            .Select(scaledColumnCoordinate => scaledColumnCoordinate + contentBounds.Left)
            .ToArray();

        // arrange the device layouts into their scaled grid positions
        for (var rowIndex = 0; rowIndex < gridRowCount; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < gridColumnCount; columnIndex++)
            {
                var fullSizeCellSize = fullSizeGridCellBounds[rowIndex, columnIndex].Size;
                if ((fullSizeCellSize.Width == 0) || (fullSizeCellSize.Height == 0))
                {
                    // if the full size bounds are zero width or height, it probably means there aren't any
                    // screens available for this device so we can't scale it to fit inside the grid cell
                    continue;
                }

                // work out the scaled coordinates for this grid cell
                var scaledGridCellBounds = new RectangleInfo(
                    x: scaledColumnCoordinates[columnIndex],
                    y: scaledRowCoordinates[rowIndex],
                    width: scaledColumnCoordinates[columnIndex + 1] - scaledColumnCoordinates[columnIndex],
                    height: scaledRowCoordinates[rowIndex + 1] - scaledRowCoordinates[rowIndex]);

                // scale and center the device layout to fit inside the grid cell
                var deviceLayout = deviceGrid[rowIndex, columnIndex];
                deviceLayout.DeviceBounds = BoxBounds.CreateFromOuterBounds(
                    outerBounds: new RectangleInfo(
                            size: fullSizeCellSize.ScaleToFit(scaledGridCellBounds.Size))
                        .Center(scaledGridCellBounds.Midpoint)
                        .Round()
                        .Intersect(scaledGridCellBounds.Round()),
                    boxStyle: deviceLayout.DeviceStyle);
            }
        }

        // verify that we're pixel-perfect on the positions
        var scaledDeviceBounds = RectangleInfo.Union(
            deviceLayouts
                .Select(deviceLayout => deviceLayout.DeviceBounds.OuterBounds));
        Debug.Assert(
            (scaledDeviceBounds.Width == contentBounds.Width) || (scaledDeviceBounds.Height == contentBounds.Height),
            string.Join(
                "\r\n",
                $"{nameof(ArrangeAndScaleDeviceLayouts)} - scaled device layout does not perfectly fill content bounds:",
                $"scaled grid = '{scaledDeviceBounds}'",
                $"content bounds = '{contentBounds}'"));
    }

    /// <summary>
    /// Arranges the screen layouts inside their respective device cells and
    /// scales them to fit inside their parent device layouts.
    /// </summary>
    private static void ArrangeAndScaleScreenLayouts(PreviewLayout.Builder previewLayout)
    {
        var deviceLayouts = previewLayout.CanvasLayout?.DeviceLayouts
            ?? throw new InvalidOperationException();

        foreach (var deviceLayout in deviceLayouts)
        {
            var deviceInfo = deviceLayout.DeviceInfo ?? throw new InvalidOperationException();
            var screenLayouts = deviceLayout.ScreenLayouts ?? throw new InvalidOperationException();
            if (screenLayouts.Count == 0)
            {
                // nothing to arrange or scale, and we'll get a divide by zero error
                // in ScaleToFitRatio if we don't quit early
                continue;
            }

            // work out the scaling factor that will fit the screen layouts into the content bounds
            var fullSizeDisplayArea = deviceInfo.GetCombinedDisplayArea();
            var contentBounds = deviceLayout.DeviceBounds.ContentBounds;
            var scalingFactor = fullSizeDisplayArea.Size.ScaleToFitRatio(contentBounds.Size);

            foreach (var screenLayout in screenLayouts)
            {
                var screenInfo = screenLayout.ScreenInfo ?? throw new InvalidOperationException();
                screenLayout.ScreenBounds = BoxBounds.CreateFromOuterBounds(
                    outerBounds: screenInfo.DisplayArea
                        .Offset(-fullSizeDisplayArea.Left, -fullSizeDisplayArea.Top)
                        .Scale(scalingFactor)
                        .Offset(contentBounds.Left, contentBounds.Top)
                        .Round()
                        .Intersect(contentBounds),
                    boxStyle: screenLayout.ScreenStyle);
            }

            // verify that we're pixel-perfect on the positions
            var scaledScreenBounds = RectangleInfo.Union(
                screenLayouts
                    .Select(screenLayout => screenLayout.ScreenBounds.OuterBounds));
            Debug.Assert(
                (scaledScreenBounds.Width == contentBounds.Width) || (scaledScreenBounds.Height == contentBounds.Height),
                string.Join(
                    "\r\n",
                    $"{nameof(ArrangeAndScaleScreenLayouts)} - scaled screen layouts do not perfectly fill content bounds:",
                    $"scaled grid = '{scaledScreenBounds}'",
                    $"content bounds = '{contentBounds}'"));
        }
    }

    /// <summary>
    /// Crops the layout's canvas bounds to tightly wrap the contained device layouts
    /// and moves it back to the same origin it started at.
    /// </summary>
    private static void ArrangeAndResizeCanvasLayout(PreviewLayout.Builder previewLayout)
    {
        var canvasLayout = previewLayout.CanvasLayout ?? throw new InvalidOperationException();

        // work out how big the canvas needs to be in order to contain the device layouts
        var canvasContentBounds = RectangleInfo.Union(
            (canvasLayout.DeviceLayouts ?? throw new InvalidOperationException())
                .Select(deviceLayout => deviceLayout.DeviceBounds.OuterBounds));
        var canvasOuterBounds = BoxBounds.CreateFromContentBounds(
            contentBounds: canvasContentBounds,
            boxStyle: canvasLayout.CanvasStyle);

        // move the canvas into position
        var positionedOuterBounds = canvasOuterBounds.OuterBounds.MoveTo(
            (canvasLayout.CanvasBounds ?? throw new InvalidOperationException())
                .OuterBounds.Location);

        canvasLayout.CanvasBounds = BoxBounds.CreateFromOuterBounds(
            outerBounds: positionedOuterBounds,
            boxStyle: canvasLayout.CanvasStyle);
    }
}
