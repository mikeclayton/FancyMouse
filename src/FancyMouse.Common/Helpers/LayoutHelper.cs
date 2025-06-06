﻿using System.Diagnostics;
using FancyMouse.Common.Models.Display;
using FancyMouse.Common.Models.Drawing;
using FancyMouse.Common.Models.Styles;
using FancyMouse.Common.Models.ViewModel;

namespace FancyMouse.Common.Helpers;

public static class LayoutHelper
{
    public static FormViewModel GetFormLayout(
        PreviewStyle previewStyle, DisplayInfo displayInfo, ScreenInfo activatedScreen, PointInfo activatedLocation)
    {
        ArgumentNullException.ThrowIfNull(previewStyle);
        ArgumentNullException.ThrowIfNull(displayInfo);

        /*

           +----------------------------[form]----------------------------+
           |▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒[canvas]▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒|
           |▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓[device 1]▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓[device 2]▓▓▓▓▓▓▒▒|
           |▒▒▓▓░░░░[screen 1]░░░░░░[screen 2]░░░░▓▓░░░░[screen 1]░░░░▓▓▒▒|
           |▒▒▓▓░░              ░░              ░░▓▓░░              ░░▓▓▒▒|
           |▒▒▓▓░░              ░░              ░░▓▓░░              ░░▓▓▒▒|
           |▒▒▓▓░░              ░░              ░░▓▓░░              ░░▓▓▒▒|
           |▒▒▓▓░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓▓░░░░░░░░░░░░░░░░░░▓▓▒▒|
           |▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░[screen 2]░░░░▓▓▒▒|
           |▒▒                                    ▓▓░░              ░░▓▓▒▒|
           |▒▒                                    ▓▓░░              ░░▓▓▒▒|
           |▒▒                                    ▓▓░░              ░░▓▓▒▒|
           |▒▒                                    ▓▓░░░░░░░░░░░░░░░░░░▓▓▒▒|
           |▒▒                                    ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▒▒|
           |▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒|
           +--------------------------------------------------------------+

        */

        // check we have at least one device
        if (displayInfo.Devices.Count == 0)
        {
            throw new ArgumentException("Value must contain at least one device.", nameof(displayInfo));
        }

        // check each device has at least one screen.
        for (var deviceIndex = 0; deviceIndex < displayInfo.Devices.Count; deviceIndex++)
        {
            var device = displayInfo.Devices[deviceIndex];
            if (device.Screens.Count == 0)
            {
                throw new ArgumentException($"{nameof(displayInfo)}.{nameof(displayInfo.Devices)}[{deviceIndex}] must contain at least one screen.", nameof(displayInfo));
            }
        }

        // work out the maximum allowed size of the preview form:
        // * can't be bigger than the activated screen
        // * can't be bigger than the configured canvas size
        var formMaxBounds = new RectangleInfo(
            previewStyle.CanvasSize
                .Clamp(activatedScreen.DisplayArea.Size));

        // create an initial form layout.
        // this is a nested structure of mutable "builder" objects that can be used to
        // build the final immutable layout objects once all the bounds have been calculated
        var formLayout = new FormViewModel.Builder
        {
            FormBounds = RectangleInfo.Empty,
            CanvasLayout = new()
            {
                CanvasBounds = BoxBounds.CreateFromOuterBounds(
                    outerBounds: formMaxBounds,
                    boxStyle: previewStyle.CanvasStyle),
                CanvasStyle = previewStyle.CanvasStyle,
                DeviceLayouts = displayInfo.Devices.Select(
                    deviceInfo => new DeviceViewModel.Builder
                    {
                        DeviceInfo = deviceInfo,
                        DeviceBounds = BoxBounds.Empty,
                        DeviceStyle = BoxStyle.Empty,
                        ScreenLayouts = deviceInfo.Screens.Select(
                            screenInfo => new ScreenViewModel.Builder
                            {
                                ScreenInfo = screenInfo,
                                ScreenBounds = BoxBounds.Empty,
                                ScreenStyle = previewStyle.ScreenStyle,
                            }).ToList(),
                    }).ToList(),
            },
        };

        // arrange the devices, screens and canvas
        LayoutHelper.ArrangeAndScaleDeviceLayouts(formLayout);
        LayoutHelper.ArrangeAndScaleScreenLayouts(formLayout);
        LayoutHelper.ArrangeAndScaleCanvasLayout(formLayout);

        // resize and center the form on the activated location
        formLayout.FormBounds = formLayout.CanvasLayout.CanvasBounds.OuterBounds
            .Center(activatedLocation)
            .MoveInside(activatedScreen.DisplayArea);

        return formLayout.Build();
    }

    /// <summary>
    /// Arranges the device layouts into a non-overlapping grid and scales them to fit inside the specified content bounds.
    /// </summary>
    internal static void ArrangeAndScaleDeviceLayouts(FormViewModel.Builder formLayout)
    {
        var deviceLayouts = formLayout?.CanvasLayout?.DeviceLayouts
            ?? throw new InvalidOperationException();
        var contentBounds = formLayout?.CanvasLayout?.CanvasBounds?.ContentBounds
            ?? throw new InvalidOperationException();

        /*

           example device layout grid:

           +-------------------------------[form]------------------------------+
           |▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒[canvas]▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒|
           |▒▒+--------------------------------------+----------------------+▒▒|
           |▒▒|                                      |▓▓▓▓▓▓[device 2]▓▓▓▓▓▓|▒▒|
           |▒▒|                                      |▓▓░░░░[screen 1]░░░░▓▓|▒▒|
           |▒▒|▓▓▓▓▓▓▓▓▓▓▓▓▓▓[device 1]▓▓▓▓▓▓▓▓▓▓▓▓▓▓|▓▓░░              ░░▓▓|▒▒|
           |▒▒|▓▓░░░░[screen 1]░░░░░░[screen 2]░░░░▓▓|▓▓░░              ░░▓▓|▒▒|
           |▒▒|▓▓░░              ░░              ░░▓▓|▓▓░░              ░░▓▓|▒▒|
           |▒▒|▓▓░░              ░░              ░░▓▓|▓▓░░░░░░░░░░░░░░░░░░▓▓|▒▒|
           |▒▒|▓▓░░              ░░              ░░▓▓|▓▓░░░░[screen 2]░░░░▓▓|▒▒|
           |▒▒|▓▓░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░▓▓|▓▓░░              ░░▓▓|▒▒|
           |▒▒|▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓|▓▓░░              ░░▓▓|▒▒|
           |▒▒|                                      |▓▓░░              ░░▓▓|▒▒|
           |▒▒|                                      |▓▓░░░░░░░░░░░░░░░░░░▓▓|▒▒|
           |▒▒|                                      |▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓|▒▒|
           |▒▒+--------------------------------------+----------------------+▒▒|
           |▒▒|        ▓▓▓▓▓▓[device 3]▓▓▓▓▓▓        |                      |▒▒|
           |▒▒|        ▓▓░░░░[screen 1]░░░░▓▓        |                      |▒▒|
           |▒▒|        ▓▓░░              ░░▓▓        |                      |▒▒|
           |▒▒|        ▓▓░░              ░░▓▓        |   ▓▓▓[device 4]▓▓▓   |▒▒|
           |▒▒|        ▓▓░░              ░░▓▓        |   ▓▓░[screen 1]░▓▓   |▒▒|
           |▒▒|        ▓▓░░░░░░░░░░░░░░░░░░▓▓        |   ▓▓░░        ░░▓▓   |▒▒|
           |▒▒|        ▓▓░░░░[screen 2]░░░░▓▓        |   ▓▓░░        ░░▓▓   |▒▒|
           |▒▒|        ▓▓░░              ░░▓▓        |   ▓▓░░░░░░░░░░░░▓▓   |▒▒|
           |▒▒|        ▓▓░░              ░░▓▓        |   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓   |▒▒|
           |▒▒|        ▓▓░░              ░░▓▓        |                      |▒▒|
           |▒▒|        ▓▓░░░░░░░░░░░░░░░░░░▓▓        |                      |▒▒|
           |▒▒|        ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓        |                      |▒▒|
           |▒▒+--------------------------------------+----------------------+▒▒|
           |▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒|
           +-------------------------------------------------------------------+

        */

        // build an initial grid at 100% scale. the grid is currently a single row
        // high with all the devices arranged left to right for the time being.
        // we'll enhance this to allow for multiple rows later to support the
        // Mouse Without Borders "square" arrangement.
        var gridRowCount = 1;
        var gridColumnCount = deviceLayouts.Count;
        var deviceGrid = new DeviceViewModel.Builder[gridRowCount, gridColumnCount];
        for (var columnIndex = 0; columnIndex < gridColumnCount; columnIndex++)
        {
            deviceGrid[0, columnIndex] = deviceLayouts[columnIndex];
        }

        // find the size of the tallest device in each row and the widest device in each column.
        // this tells us how tall each row needs to be and how wide each column needs to be.
        var fullSizeBounds = new RectangleInfo[gridRowCount, gridColumnCount];
        var fullSizeRowHeights = new decimal[gridRowCount];
        var fullSizeColumnWidths = new decimal[gridColumnCount];
        for (var rowIndex = 0; rowIndex < gridRowCount; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < gridColumnCount; columnIndex++)
            {
                var deviceInfo = deviceGrid[rowIndex, columnIndex].DeviceInfo ?? throw new InvalidOperationException();
                var deviceBounds = deviceInfo.GetCombinedDisplayArea();
                fullSizeBounds[rowIndex, columnIndex] = deviceBounds;
                fullSizeRowHeights[rowIndex] = Math.Max(fullSizeRowHeights[rowIndex], deviceBounds.Height);
                fullSizeColumnWidths[columnIndex] = Math.Max(fullSizeColumnWidths[columnIndex], deviceBounds.Width);
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
        // coordinate to simplify building the end cell of each row and column in the scaled grid.
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
                // work out the scaled coordinates for this grid cell
                var gridCellBounds = new RectangleInfo(
                    x: scaledColumnCoordinates[columnIndex],
                    y: scaledRowCoordinates[rowIndex],
                    width: scaledColumnCoordinates[columnIndex + 1] - scaledColumnCoordinates[columnIndex],
                    height: scaledRowCoordinates[rowIndex + 1] - scaledRowCoordinates[rowIndex]);

                // scale and center the device layout to fit inside the grid cell
                var deviceLayout = deviceGrid[rowIndex, columnIndex];
                deviceLayout.DeviceBounds = BoxBounds.CreateFromOuterBounds(
                    outerBounds: new RectangleInfo(
                            size: fullSizeBounds[rowIndex, columnIndex].Size.ScaleToFit(gridCellBounds.Size))
                        .Center(gridCellBounds.Midpoint)
                        .Round()
                        .Intersect(gridCellBounds.Round()),
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
    /// Arranges the screen layouts and scales them to fit inside their parent device layouts.
    /// </summary>
    internal static void ArrangeAndScaleScreenLayouts(FormViewModel.Builder formLayout)
    {
        var deviceLayouts = formLayout?.CanvasLayout?.DeviceLayouts
            ?? throw new InvalidOperationException();
        foreach (var deviceLayout in deviceLayouts)
        {
            var deviceInfo = deviceLayout.DeviceInfo ?? throw new InvalidOperationException();
            var screenLayouts = deviceLayout.ScreenLayouts ?? throw new InvalidOperationException();
            var contentBounds = deviceLayout.DeviceBounds.ContentBounds;

            // work out the scaling factor that will fit the screen layouts into the content bounds
            var fullSizeDisplayArea = deviceInfo.GetCombinedDisplayArea();
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

    internal static void ArrangeAndScaleCanvasLayout(FormViewModel.Builder formLayout)
    {
        var canvasLayout = formLayout?.CanvasLayout ?? throw new InvalidOperationException();

        // work out how big the canvas needs to be in order to contain the device layouts
        var canvasContentBounds = RectangleInfo.Union(
            (canvasLayout.DeviceLayouts ?? throw new InvalidOperationException())
                .Select(deviceLayout => deviceLayout.DeviceBounds.OuterBounds));
        var canvasOuterBounds = BoxBounds.CreateFromContentBounds(
            contentBounds: canvasContentBounds,
            boxStyle: formLayout.CanvasLayout.CanvasStyle);

        // move the canvas into position
        var positionedOuterBounds = canvasOuterBounds.OuterBounds.MoveTo(
            (canvasLayout.CanvasBounds ?? throw new InvalidOperationException())
                .OuterBounds.Location);

        canvasLayout.CanvasBounds = BoxBounds.CreateFromOuterBounds(
            outerBounds: positionedOuterBounds,
            boxStyle: formLayout.CanvasLayout.CanvasStyle);
    }
}
