using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using FancyMouse.Helpers.Screenshot;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;

namespace FancyMouse.Helpers;

internal static class DrawingHelper
{
    internal static Bitmap RenderPreview(
        PreviewLayout previewLayout,
        IScreenshotProvider screenshotProvider)
    {
        return DrawingHelper.RenderPreview(previewLayout, screenshotProvider, null, null);
    }

    internal static Bitmap RenderPreview(
        PreviewLayout previewLayout,
        IScreenshotProvider screenshotProvider,
        Action<Bitmap>? previewImageCreatedCallback,
        Action? previewImageUpdatedCallback)
    {
        var stopwatch = Stopwatch.StartNew();

        // initialize the preview image
        var previewBounds = previewLayout.PreviewBounds.OuterBounds.ToRectangle();
        var previewImage = new Bitmap(previewBounds.Width, previewBounds.Height, PixelFormat.Format32bppArgb);
        var previewGraphics = Graphics.FromImage(previewImage);
        previewImageCreatedCallback?.Invoke(previewImage);

        DrawingHelper.DrawBoxBorder(previewGraphics, previewLayout.PreviewStyle.CanvasStyle, previewLayout.PreviewBounds);
        DrawingHelper.DrawBoxBackground(
            previewGraphics,
            previewLayout.PreviewStyle.CanvasStyle,
            previewLayout.PreviewBounds,
            Enumerable.Empty<RectangleInfo>());

        // sort the source and target screen areas into the order we want to
        // draw them, putting the activated screen first (we need to capture
        // and draw the activated screen before we show the form because
        // otherwise we'll capture the form as part of the screenshot!)
        var sourceScreens = new List<RectangleInfo> { previewLayout.Screens[previewLayout.ActivatedScreenIndex] }
            .Union(previewLayout.Screens.Where((_, idx) => idx != previewLayout.ActivatedScreenIndex))
            .ToList();
        var targetScreens = new List<BoxBounds> { previewLayout.ScreenshotBounds[previewLayout.ActivatedScreenIndex] }
            .Union(previewLayout.ScreenshotBounds.Where((_, idx) => idx != previewLayout.ActivatedScreenIndex))
            .ToList();

        // draw all the screenshot bezels
        foreach (var screenshotBounds in previewLayout.ScreenshotBounds)
        {
            DrawingHelper.DrawBoxBorder(
                previewGraphics, previewLayout.PreviewStyle.ScreenshotStyle, screenshotBounds);
        }

        var imageUpdated = false;
        var placeholdersDrawn = false;
        for (var i = 0; i < sourceScreens.Count; i++)
        {
            screenshotProvider.DrawScreenshot(previewGraphics, sourceScreens[i], targetScreens[i].ContentBounds);
            imageUpdated = true;

            // show the placeholder images and show the form if it looks like it might take
            // a while to capture the remaining screenshot images (but only if there are any)
            if (stopwatch.ElapsedMilliseconds > 250)
            {
                // draw placeholder backgrounds for any undrawn screens
                if (!placeholdersDrawn)
                {
                    DrawingHelper.DrawPlaceholders(
                        previewGraphics,
                        previewLayout.PreviewStyle.ScreenshotStyle,
                        targetScreens.GetRange(i + 1, targetScreens.Count - i - 1));
                    placeholdersDrawn = true;
                }

                previewImageUpdatedCallback?.Invoke();
                imageUpdated = false;
            }
        }

        if (imageUpdated)
        {
            previewImageUpdatedCallback?.Invoke();
        }

        stopwatch.Stop();

        return previewImage;
    }

    /// <summary>
    /// Draws a border shape with an optional 3d highlight and shadow effect.
    /// </summary>
    public static void DrawBoxBorder(
        Graphics graphics, BoxStyle boxStyle, BoxBounds boxBounds)
    {
        var borderInfo = boxStyle.BorderStyle;
        if (borderInfo is { Horizontal: 0, Vertical: 0 })
        {
            return;
        }

        // draw the preview border
        using var borderBrush = new SolidBrush(borderInfo.Color);
        var borderRegion = new Region(boxBounds.BorderBounds.ToRectangle());
        borderRegion.Exclude(boxBounds.PaddingBounds.ToRectangle());
        graphics.FillRegion(borderBrush, borderRegion);

        // draw the highlight and shadow
        var bounds = boxBounds.BorderBounds.ToRectangle();
        using var highlight = new Pen(Color.FromArgb(0x44, 0xFF, 0xFF, 0xFF));
        using var shadow = new Pen(Color.FromArgb(0x44, 0x00, 0x00, 0x00));

        for (var i = 0; i < borderInfo.Depth; i++)
        {
            // left edge
            if (borderInfo.Left >= i * 2)
            {
                graphics.DrawLine(
                    highlight,
                    bounds.Left + i,
                    bounds.Top + i,
                    bounds.Left + i,
                    bounds.Bottom - 1 - i);
                graphics.DrawLine(
                    shadow,
                    bounds.Left + (int)borderInfo.Left - 1 - i,
                    bounds.Top + (int)borderInfo.Top - 1 - i,
                    bounds.Left + (int)borderInfo.Left - 1 - i,
                    bounds.Bottom - (int)borderInfo.Bottom + i);
            }

            // top edge
            if (borderInfo.Top >= i * 2)
            {
                graphics.DrawLine(
                    highlight,
                    bounds.Left + i,
                    bounds.Top + i,
                    bounds.Right - 1 - i,
                    bounds.Top + i);
                graphics.DrawLine(
                    shadow,
                    bounds.Left + (int)borderInfo.Left - 1 - i,
                    bounds.Top + (int)borderInfo.Top - 1 - i,
                    bounds.Right - (int)borderInfo.Right + i,
                    bounds.Top + (int)borderInfo.Bottom - 1 - i);
            }

            // right edge
            if (borderInfo.Right >= i * 2)
            {
                graphics.DrawLine(
                    highlight,
                    bounds.Right - (int)borderInfo.Right + i,
                    bounds.Top + (int)borderInfo.Top - 1 - i,
                    bounds.Right - (int)borderInfo.Right + i,
                    bounds.Bottom - (int)borderInfo.Bottom + i);
                graphics.DrawLine(
                    shadow,
                    bounds.Right - 1 - i,
                    bounds.Top + i,
                    bounds.Right - 1 - i,
                    bounds.Bottom - 1 - i);
            }

            // bottom edge
            if (borderInfo.Bottom >= i * 2)
            {
                graphics.DrawLine(
                    highlight,
                    bounds.Left + (int)borderInfo.Left - 1 - i,
                    bounds.Bottom - (int)borderInfo.Bottom + i,
                    bounds.Right - (int)borderInfo.Right + i,
                    bounds.Bottom - (int)borderInfo.Bottom + i);
                graphics.DrawLine(
                    shadow,
                    bounds.Left + i,
                    bounds.Bottom - 1 - i,
                    bounds.Right - 1 - i,
                    bounds.Bottom - 1 - i);
            }
        }
    }

    /// <summary>
    /// Draws a gradient-filled background shape.
    /// </summary>
    public static void DrawBoxBackground(
        Graphics graphics, BoxStyle boxStyle, BoxBounds boxBounds, IEnumerable<RectangleInfo> excludeBounds)
    {
        var backgroundBounds = boxBounds.PaddingBounds;
        var backgroundInfo = boxStyle.BackgroundStyle;

        using var backgroundBrush = new LinearGradientBrush(
            backgroundBounds.ToRectangle(),
            backgroundInfo.Color1,
            backgroundInfo.Color2,
            LinearGradientMode.ForwardDiagonal);

        var backgroundRegion = new Region(backgroundBounds.ToRectangle());

        // it's faster to build a region with the screen areas excluded
        // and fill that than it is to fill the entire bounding rectangle
        foreach (var exclude in excludeBounds)
        {
            backgroundRegion.Exclude(exclude.ToRectangle());
        }

        graphics.FillRegion(backgroundBrush, backgroundRegion);
    }

    /// <summary>
    /// Draws placeholder background images for the specified screens on the preview.
    /// </summary>
    public static void DrawPlaceholders(
        Graphics graphics, BoxStyle screenStyle, IList<BoxBounds> screenBounds)
    {
        if (screenBounds.Any())
        {
            var brush = new SolidBrush(screenStyle.BackgroundStyle.Color1);
            graphics.FillRectangles(brush, screenBounds.Select(bounds => bounds.PaddingBounds.ToRectangle()).ToArray());
        }
    }
}
