using System.Drawing;
using System.Drawing.Imaging;

using FancyMouse.Drawing.Capture;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;

namespace FancyMouse.Drawing.Helpers;

/// <summary>
/// Composes a <see cref="PreviewStyle"/> into a single flattened bitmap - outer border, background,
/// and per-screen bezels/screenshots all baked into one image, with each screen's content
/// supplied by <paramref name="captureProvider"/> (see <see cref="DrawPreviewAsync"/>). This is a
/// fallback for contexts that can't render the layout as native, separately-composited UI
/// elements the way <c>FancyMouse.WinUI3</c>'s <c>PreviewWindow</c>/<c>PreviewPane</c> do -
/// currently used by <c>FancyMouse.SettingsUI</c>'s live style preview (against a bundled
/// fake-desktop image, via <see cref="StaticScreenshotCaptureProvider"/>) and by
/// <c>DrawingHelperTests</c> (to keep comparing against golden images captured under this same
/// flattened layout).
/// </summary>
public static class PreviewHelper
{
    /// <summary>
    /// Builds a static bitmap containing a composed view of the borders, backgrounds, bezels
    /// and screenshots for a pop-up preview. This is a static bitmap representation of the WinUI
    /// PreviewWindow form's controls, and can be used as the "live preview" in the SettingsUI
    /// and in tests to confirm the visual components all draw correctly compared to a reference image.
    /// </summary>
    public static async Task<Bitmap> DrawPreviewAsync(
        PreviewStyle previewStyle,
        IScreenshotCaptureProvider captureProvider,
        DisplayInfo displayInfo,
        SizeInfo? maximumSize = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(previewStyle);
        ArgumentNullException.ThrowIfNull(captureProvider);
        ArgumentNullException.ThrowIfNull(displayInfo);

        var previewLayout = LayoutHelper.GetPreviewLayout(previewStyle, displayInfo, maximumSize);
        var canvasLayout = previewLayout.CanvasLayout;

        // in the WinUI PreviewWindow, the entire visualisation is a PreviewPane (which contains
        // the outer margin and border, main background, bezels and screenshots), and the pane's
        // layout is relative to its own top-left corner - so its coordinates are the absolute
        // position in the rendered image, and the outer margin is left transparent
        var bounds = new RectangleInfo(previewLayout.PreviewSize).ToRectangle();
        var previewImage = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(previewImage);
        graphics.Clear(Color.Transparent);

        var canvasBounds = canvasLayout.CanvasBounds;
        using (var border = DrawingHelper.RenderBorder(canvasBounds.BorderBounds.Size, canvasLayout.CanvasStyle.BorderStyle))
        {
            graphics.DrawImageUnscaled(border, (int)canvasBounds.BorderBounds.X, (int)canvasBounds.BorderBounds.Y);
        }

        using (var background = DrawingHelper.RenderBackground(canvasBounds.PaddingBounds.Size, canvasLayout.CanvasStyle.BackgroundStyle))
        {
            graphics.DrawImageUnscaled(background, (int)canvasBounds.PaddingBounds.X, (int)canvasBounds.PaddingBounds.Y);
        }

        foreach (var screenLayout in canvasLayout.DeviceLayouts.SelectMany(deviceLayout => deviceLayout.ScreenLayouts))
        {
            var screenBounds = screenLayout.ScreenBounds;

            using (var bezel = DrawingHelper.RenderBorder(screenBounds.BorderBounds.Size, screenLayout.ScreenStyle.BorderStyle))
            {
                graphics.DrawImageUnscaled(bezel, (int)screenBounds.BorderBounds.X, (int)screenBounds.BorderBounds.Y);
            }

            // the capture provider owns anti-bleed/interpolation settings for its own thumbnail
            // (see StaticScreenshotCaptureProvider/DesktopScreenshotCaptureProvider) - it always
            // hands back a bitmap already sized to screenBounds.ContentBounds.Size, so this is a
            // plain 1:1 paste, not a scaled draw.
            using var screenshotImage = await captureProvider.CaptureAsync(
                screenLayout.ScreenInfo.DisplayArea,
                screenBounds.ContentBounds.Size,
                cancellationToken).ConfigureAwait(false);
            graphics.DrawImageUnscaled(
                screenshotImage,
                (int)screenBounds.ContentBounds.X,
                (int)screenBounds.ContentBounds.Y);
        }

        return previewImage;
    }
}
