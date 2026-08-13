using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using FancyMouse.Common.Imaging;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;

namespace FancyMouse.Common.Helpers;

public static class DrawingHelper
{
    /// <summary>
    /// Renders the device/screen bezels and screenshots for the specified canvas layout -
    /// deliberately excludes the background fill and the outer border, which are rendered
    /// separately (see <see cref="RenderBackground"/>/<see cref="RenderBorder"/>) since a
    /// hosting window may layer them independently (e.g. a WinUI3 host renders its own
    /// border, and rendering the background as its own image lets it stay cached/reused
    /// independently of the screenshots later).
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
    /// An image of the bezels and screenshots for the canvas layout.
    /// </returns>
    public static async Task<Bitmap> RenderPreviewAsync(
        CanvasLayout canvasLayout,
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
                previewGraphics, screenDrawingOp.ScreenLayout.ScreenBounds, screenDrawingOp.ScreenLayout.ScreenStyle);
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

    /// <summary>
    /// Renders the gradient-filled background for the specified canvas layout, as its own
    /// transparent-elsewhere image sized to <paramref name="canvasLayout"/>'s own outer
    /// bounds - a hosting window layers this beneath <see cref="RenderPreviewAsync"/>'s
    /// output (and, separately, its own border image) rather than having it baked into a
    /// single flattened bitmap.
    /// </summary>
    public static Bitmap RenderBackground(CanvasLayout canvasLayout)
    {
        var bounds = canvasLayout.CanvasBounds.OuterBounds.ToRectangle();
        var image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Transparent);
        DrawingHelper.DrawBackgroundFill(
            graphics,
            canvasLayout.CanvasStyle,
            canvasLayout.CanvasBounds,
            []);
        return image;
    }

    /// <summary>
    /// Renders a raised border ring for the specified box, as its own transparent-elsewhere
    /// image sized to <paramref name="hostBounds"/>'s outer bounds - used by a hosting
    /// window to render its own border independently of whatever it's hosting (see
    /// <see cref="LayoutHelper.GetHostBoxStyle"/>/<see cref="LayoutHelper.GetHostBounds"/>).
    /// </summary>
    /// <remarks>
    /// <paramref name="hostBounds"/> must be zero-based (<c>OuterBounds.Location == (0,0)</c>)
    /// - its rectangles are used directly as pixel coordinates into the bitmap this
    /// allocates, which is sized to exactly fit it. Callers deriving a host box by enlarging
    /// a zero-based content box (<see cref="LayoutHelper.GetHostBounds"/>) need to
    /// re-anchor it first - e.g. <c>hostBounds.MoveTo(new PointInfo(0, 0))</c> - since
    /// enlarging outward shifts the origin negative.
    /// </remarks>
    public static Bitmap RenderBorder(BoxBounds hostBounds, BoxStyle hostStyle)
    {
        var bounds = hostBounds.OuterBounds.ToRectangle();
        var image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Transparent);
        DrawingHelper.DrawRaisedBorder(graphics, hostBounds, hostStyle);
        return image;
    }

    /// <summary>
    /// Renders the border, background, and bezels/screenshots for a <see cref="PreviewLayout"/>
    /// into a single flattened image - the pre-split rendering behavior, kept for hosts (the
    /// legacy WinForms app) that display the preview as one image rather than layering the
    /// three pieces independently.
    /// </summary>
    public static async Task<Bitmap> RenderCombinedPreviewAsync(
        PreviewLayout previewLayout,
        BoxStyle hostBoxStyle,
        ScreenInfo activatedScreen,
        List<IImageRegionCopyService> imageRegionCopyServices,
        Func<Bitmap, Task>? previewImageCreatedCallback = null,
        Func<Bitmap, Task>? previewImageUpdatedCallback = null)
    {
        var canvasLayout = previewLayout.CanvasLayout;
        var hostBounds = LayoutHelper.GetHostBounds(canvasLayout.CanvasBounds.OuterBounds, hostBoxStyle)
            .MoveTo(new PointInfo(0, 0));

        // hostBounds.OuterBounds is now (0,0)-based, so hostBounds.ContentBounds.Location is
        // exactly the margin+border offset to draw the (also zero-based) canvas-relative
        // background/screenshots images at, within the host-sized combined bitmap.
        var offsetX = (int)hostBounds.ContentBounds.X;
        var offsetY = (int)hostBounds.ContentBounds.Y;

        using var borderImage = DrawingHelper.RenderBorder(hostBounds, hostBoxStyle);
        using var backgroundImage = DrawingHelper.RenderBackground(canvasLayout);

        var combinedBounds = hostBounds.OuterBounds.ToRectangle();
        var combinedImage = new Bitmap(combinedBounds.Width, combinedBounds.Height, PixelFormat.Format32bppPArgb);

        // KNOWN ISSUE - spam-activating the host (e.g. mashing the hotkey) can start a new
        // ShowPreviewAsync() call while a previous one is still rendering. Nothing here
        // serializes/cancels overlapping calls, and each caller's own ClearPreview()-style
        // cleanup disposes whatever Bitmap is currently on display - which can be *this*
        // combinedImage, still owned by an earlier, still-in-flight call to this method.
        // Graphics.FromImage(combinedImage) below then throws ArgumentException ("Parameter
        // is not valid") because the underlying GDI+ handle is already gone.
        // Deferred rather than patched here: this whole progressive-callback/shared-bitmap
        // design is expected to be replaced by the "stage 2" per-screen capture pipeline
        // (see the FancyMouse.WinUI3 PreviewWindow/PreviewPane split), at which point this
        // shared-mutable-Bitmap shape goes away anyway - any interim locking/cancellation
        // fix here would just be thrown away with it.
        void Compose(Bitmap screenshotsImage)
        {
            using var combinedGraphics = Graphics.FromImage(combinedImage);
            combinedGraphics.Clear(Color.Transparent);
            combinedGraphics.DrawImageUnscaled(borderImage, 0, 0);
            combinedGraphics.DrawImageUnscaled(backgroundImage, offsetX, offsetY);
            combinedGraphics.DrawImageUnscaled(screenshotsImage, offsetX, offsetY);
        }

        using var screenshotsImage = await DrawingHelper.RenderPreviewAsync(
            canvasLayout,
            activatedScreen,
            imageRegionCopyServices,
            previewImageCreatedCallback: async screenshots =>
            {
                Compose(screenshots);
                if (previewImageCreatedCallback != null)
                {
                    await previewImageCreatedCallback(combinedImage);
                }
            },
            previewImageUpdatedCallback: async screenshots =>
            {
                Compose(screenshots);
                if (previewImageUpdatedCallback != null)
                {
                    await previewImageUpdatedCallback(combinedImage);
                }
            }).ConfigureAwait(false);

        Compose(screenshotsImage);

        return combinedImage;
    }

    // Hardcoded 3-D lighting config shared by all bezels.
    // Values are compile-time constants; BezelConfig exists so the rendering
    // methods are parameterised and ready to accept per-bezel variation later.
    private static readonly Drawing.ContouredBezels.BezelConfig ContouredBezelConfig = new(
        fadeStart: 30.0,            // degrees from edge where corner rolloff begins
        fadeEnd: 60.0,              // degrees where rolloff reaches zero
        highlightMax: 0x44 / 255.0, // peak highlight opacity (~26.7 %)
        shadowMax: 0x44 / 255.0,    // peak shadow   opacity (~26.7 %)
        edgeFadeFraction: 0.75f,    // fraction of edge length with secondary effect
        rampAngleDegrees: 45.0);    // chamfer inclination — cos(45°) ≈ 0.707 uniform intensity

    /// <summary>
    /// Draws a border shape with a raised 3-D highlight and shadow effect.
    /// </summary>
    private static void DrawRaisedBorder(
        Graphics graphics, BoxBounds boxBounds, BoxStyle boxStyle)
    {
        var borderStyle = boxStyle.BorderStyle;
        if ((borderStyle.Horizontal < 1) || (borderStyle.Vertical < 1))
        {
            return;
        }

        if (borderStyle.Color is null)
        {
            return;
        }

        // draw a contoured bezel
        var bounds = boxBounds.BorderBounds.ToRectangle();
        using var renderer = new Drawing.ContouredBezels.BezelRenderer(
            borderStyle,
            DrawingHelper.ContouredBezelConfig);
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
