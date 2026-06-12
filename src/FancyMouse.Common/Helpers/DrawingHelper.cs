using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using FancyMouse.Common.Imaging;
using FancyMouse.Drawing.Bezels;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;
using FancyMouse.Models.ViewModel;
using NLog;

namespace FancyMouse.Common.Helpers;

public static class DrawingHelper
{
    /// <summary>
    /// Renders a preview image of the specified canvas layout.
    /// </summary>
    /// <param name="canvasLayout">
    /// The layout of the canvas, including the layout of all devices and screens.
    /// </param>
    /// <param name="activatedScreen">
    /// The screen that is currently activated (i.e. the one that the user is interacting with).
    /// </param>
    /// <param name="imageRegionCopyServices">
    /// A list of IImageRegionCopyService implementations, one for each device in the canvas layout.
    /// </param>
    /// <param name="previewImageCreatedCallback">
    /// A callback that is invoked when the preview image is created.
    /// </param>
    /// <param name="previewImageUpdatedCallback">
    /// A callback that is invoked when the preview image is updated.
    /// </param>
    /// <returns>
    /// A preview image of the canvas layout.
    /// </returns>
    public static async Task<Bitmap> RenderPreviewAsync(
        ILogger logger,
        CanvasViewModel canvasLayout,
        ScreenInfo activatedScreen,
        List<IImageRegionCopyService> imageRegionCopyServices,
        Func<Bitmap, Task>? previewImageCreatedCallback = null,
        Func<Bitmap, Task>? previewImageUpdatedCallback = null)
    {
        var stopwatch = Stopwatch.StartNew();

        // initialize the preview image
        var previewBounds = canvasLayout.CanvasBounds.OuterBounds.ToRectangle();
        var previewImage = new Bitmap(previewBounds.Width, previewBounds.Height, PixelFormat.Format32bppPArgb);
        var previewGraphics = Graphics.FromImage(previewImage);
        previewGraphics.Clear(Color.Transparent);
        if (previewImageCreatedCallback != null)
        {
            await previewImageCreatedCallback(previewImage);
        }

        DrawingHelper.DrawRaisedBorder(logger, previewGraphics, canvasLayout.CanvasBounds, canvasLayout.CanvasStyle);
        DrawingHelper.DrawBackgroundFill(
            previewGraphics,
            canvasLayout.CanvasStyle,
            canvasLayout.CanvasBounds,
            []);

        // sort the source and target screen areas into the order we want to
        // draw them, putting the activated screen first (we need to capture
        // and draw the activated screen before we show the form because
        // otherwise we'll capture the form as part of the screenshot!)
        var screenDrawingOps = canvasLayout.DeviceLayouts
            .SelectMany(
                (deviceLayout, deviceIndex) => deviceLayout.ScreenLayouts.Select(
                    screenLayout => new
                    {
                        DeviceIndex = deviceIndex,
                        DeviceLayout = deviceLayout,
                        ScreenLayout = screenLayout,
                        CopyService = imageRegionCopyServices[deviceIndex],
                    }))
            .OrderByDescending(
                pair => object.ReferenceEquals(pair.ScreenLayout, activatedScreen))
            .ToList();

        // draw all the screenshot bezels
        foreach (var screenDrawingOp in screenDrawingOps)
        {
            DrawingHelper.DrawRaisedBorder(
                logger, previewGraphics, screenDrawingOp.ScreenLayout.ScreenBounds, screenDrawingOp.ScreenLayout.ScreenStyle);
        }

        var refreshRequired = false;
        var placeholdersDrawn = false;
        for (var i = 0; i < screenDrawingOps.Count; i++)
        {
            var screenDrawingOp = screenDrawingOps[i];

            screenDrawingOp.CopyService.CopyImageRegion(
                targetGraphics: previewGraphics,
                sourceBounds: screenDrawingOp.ScreenLayout.ScreenInfo.DisplayArea,
                targetBounds: screenDrawingOp.ScreenLayout.ScreenBounds.ContentBounds);
            refreshRequired = true;

            // show the placeholder images and show the form if it looks like it might take
            // a while to capture the remaining screenshot images (but only if there are any)
            if (stopwatch.ElapsedMilliseconds > 250)
            {
                // draw placeholder backgrounds for any undrawn screens
                if (!placeholdersDrawn)
                {
                    DrawingHelper.DrawScreenPlaceholders(
                        previewGraphics,
                        screenDrawingOp.ScreenLayout.ScreenStyle,
                        screenDrawingOps
                            .Skip(i + 1)
                            .Select(drawingOp => drawingOp.ScreenLayout.ScreenBounds)
                            .ToList());
                    placeholdersDrawn = true;
                }

                if (previewImageUpdatedCallback != null)
                {
                    await previewImageUpdatedCallback(previewImage);
                }

                refreshRequired = false;
            }
        }

        if (refreshRequired)
        {
            previewImageUpdatedCallback?.Invoke(previewImage);
        }

        stopwatch.Stop();

        return previewImage;
    }

    // Hardcoded 3-D lighting config shared by all bezels.
    // Values are compile-time constants; BezelConfig exists so the rendering
    // methods are parameterised and ready to accept per-bezel variation later.
    private static readonly BezelConfig DefaultBezelConfig = new(
        fadeStart: 30.0,           // degrees from edge where corner rolloff begins
        fadeEnd: 60.0,             // degrees where rolloff reaches zero
        highlightMax: 0x44 / 255.0, // peak highlight opacity (~26.7 %)
        shadowMax: 0x44 / 255.0,   // peak shadow   opacity (~26.7 %)
        edgeFadeFraction: 0.75f,   // fraction of edge length with secondary effect
        rampAngleDegrees: 45.0);   // chamfer inclination — cos(45°) ≈ 0.707 uniform intensity

    /// <summary>
    /// Draws a border shape with a raised 3-D highlight and shadow effect.
    /// </summary>
    private static void DrawRaisedBorder(
        ILogger logger, Graphics graphics, BoxBounds boxBounds, BoxStyle boxStyle)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        var borderStyle = boxStyle.BorderStyle;
        if ((borderStyle.Horizontal < 1) || (borderStyle.Vertical < 1))
        {
            return;
        }

        if (borderStyle.Color is null)
        {
            return;
        }

        var bounds = boxBounds.BorderBounds.ToRectangle();
        using var renderer = new BezelRenderer(logger, borderStyle, DrawingHelper.DefaultBezelConfig);
        renderer.DrawBezel(graphics, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    /// <summary>
    /// Draws a gradient-filled background shape.
    /// </summary>
    private static void DrawBackgroundFill(
        Graphics graphics, BoxStyle boxStyle, BoxBounds boxBounds, IEnumerable<RectangleInfo> excludeBounds)
    {
        var backgroundBounds = boxBounds.PaddingBounds;

        using var backgroundBrush = DrawingHelper.GetBackgroundStyleBrush(boxStyle.BackgroundStyle, backgroundBounds);
        if (backgroundBrush == null)
        {
            return;
        }

        // it's faster to build a region with the screen areas excluded
        // and fill that than it is to fill the entire bounding rectangle
        var backgroundRegion = new Region(backgroundBounds.ToRectangle());
        foreach (var exclude in excludeBounds)
        {
            backgroundRegion.Exclude(exclude.ToRectangle());
        }

        graphics.FillRegion(backgroundBrush, backgroundRegion);
    }

    /// <summary>
    /// Draws placeholder background images for the specified screens on the preview.
    /// </summary>
    private static void DrawScreenPlaceholders(
        Graphics graphics, BoxStyle screenStyle, List<BoxBounds> screenBounds)
    {
        if (screenBounds.Count == 0)
        {
            return;
        }

        if (screenStyle.BackgroundStyle.Color1 == null)
        {
            return;
        }

        using var brush = new SolidBrush(screenStyle.BackgroundStyle.Color1.Value);
        graphics.FillRectangles(brush, screenBounds.Select(bounds => bounds.PaddingBounds.ToRectangle()).ToArray());
    }

    private static Brush? GetBackgroundStyleBrush(BackgroundStyle backgroundStyle, RectangleInfo backgroundBounds)
    {
        var backgroundBrush = backgroundStyle switch
        {
            { Color1: not null, Color2: not null } =>
                /* draw a gradient fill if both colors are specified */
                new LinearGradientBrush(
                    backgroundBounds.ToRectangle(),
                    backgroundStyle.Color1.Value,
                    backgroundStyle.Color2.Value,
                    LinearGradientMode.ForwardDiagonal),
            { Color1: not null } =>
                /* draw a solid fill if only one color is specified */
                new SolidBrush(
                    backgroundStyle.Color1.Value),
            { Color2: not null } =>
                /* draw a solid fill if only one color is specified */
                new SolidBrush(
                    backgroundStyle.Color2.Value),
            _ => (Brush?)null,
        };
        return backgroundBrush;
    }
}
