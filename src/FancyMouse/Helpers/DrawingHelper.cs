using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;
using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.Helpers;

internal static class DrawingHelper
{
    internal static Bitmap RenderPreview(
        PreviewLayout previewLayout,
        HDC sourceHdc)
    {
        return DrawingHelper.RenderPreview(previewLayout, sourceHdc, null, null);
    }

    internal static Bitmap RenderPreview(
        PreviewLayout previewLayout,
        HDC sourceHdc,
        Action<Bitmap>? previewImageCreatedCallback,
        Action? previewImageUpdatedCallback)
    {
        var stopwatch = Stopwatch.StartNew();

        // initialize the preview image
        var previewBounds = previewLayout.PreviewBounds.OuterBounds.ToRectangle();
        var previewImage = new Bitmap(previewBounds.Width, previewBounds.Height, PixelFormat.Format32bppArgb);
        var previewGraphics = Graphics.FromImage(previewImage);
        previewImageCreatedCallback?.Invoke(previewImage);

        DrawingHelper.DrawBoxBorder(previewGraphics, previewLayout.PreviewStyle, previewLayout.PreviewBounds);
        DrawingHelper.DrawBoxBackground(
            previewGraphics,
            previewLayout.PreviewStyle,
            previewLayout.PreviewBounds,
            Enumerable.Empty<RectangleInfo>());

        var previewHdc = HDC.Null;
        var imageUpdated = false;
        try
        {
            // sort the source and target screen areas, putting the activated screen first
            // (we need to capture and draw the activated screen before we show the form
            // because otherwise we'll capture the form as part of the screenshot!)
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
                    previewGraphics, previewLayout.ScreenshotStyle, screenshotBounds);
            }

            var placeholdersDrawn = false;
            for (var i = 0; i < sourceScreens.Count; i++)
            {
                DrawingHelper.EnsurePreviewDeviceContext(previewGraphics, ref previewHdc);
                DrawingHelper.DrawScreenshot(
                    sourceHdc, previewHdc, sourceScreens[i], targetScreens[i]);
                imageUpdated = true;

                // show the placeholder images and show the form if it looks like it might take
                // a while to capture the remaining screenshot images (but only if there are any)
                if ((i >= (sourceScreens.Count - 1)) || (stopwatch.ElapsedMilliseconds <= 250))
                {
                    continue;
                }

                // we need to release the device context handle before we draw anything
                // using the Graphics object otherwise we'll get an error from GDI saying
                // "Object is currently in use elsewhere"
                DrawingHelper.FreePreviewDeviceContext(previewGraphics, ref previewHdc);

                if (!placeholdersDrawn)
                {
                    // draw placeholders for any undrawn screens
                    DrawingHelper.DrawPlaceholders(
                        previewGraphics,
                        previewLayout.ScreenshotStyle,
                        targetScreens.Where((_, idx) => idx > i).ToList());
                    placeholdersDrawn = true;
                }

                previewImageUpdatedCallback?.Invoke();
                imageUpdated = false;
            }
        }
        finally
        {
            DrawingHelper.FreePreviewDeviceContext(previewGraphics, ref previewHdc);
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

    public static void EnsureDesktopDeviceContext(ref HWND desktopHwnd, ref HDC desktopHdc)
    {
        if (desktopHwnd.IsNull)
        {
            desktopHwnd = User32.GetDesktopWindow();
        }

        if (desktopHdc.IsNull)
        {
            desktopHdc = User32.GetWindowDC(desktopHwnd);
            if (desktopHdc.IsNull)
            {
                throw new InvalidOperationException(
                    $"{nameof(User32.GetWindowDC)} returned null");
            }
        }
    }

    public static void FreeDesktopDeviceContext(ref HWND desktopHwnd, ref HDC desktopHdc)
    {
        if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
        {
            var result = User32.ReleaseDC(desktopHwnd, desktopHdc);
            if (result == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(User32.ReleaseDC)} returned {result}");
            }
        }

        desktopHwnd = HWND.Null;
        desktopHdc = HDC.Null;
    }

    /// <summary>
    /// Checks if the device context handle exists, and creates a new one from the
    /// specified Graphics object if not.
    /// </summary>
    public static void EnsurePreviewDeviceContext(Graphics graphics, ref HDC previewHdc)
    {
        if (previewHdc.IsNull)
        {
            previewHdc = (HDC)graphics.GetHdc();
            var result = Gdi32.SetStretchBltMode(previewHdc, Gdi32.STRETCH_BLT_MODE.STRETCH_HALFTONE);

            if (result == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(Gdi32.SetStretchBltMode)} returned {result}");
            }
        }
    }

    /// <summary>
    /// Free the specified device context handle if it exists.
    /// </summary>
    public static void FreePreviewDeviceContext(Graphics graphics, ref HDC previewHdc)
    {
        if ((graphics is not null) && !previewHdc.IsNull)
        {
            graphics.ReleaseHdc(previewHdc.Value);
            previewHdc = HDC.Null;
        }
    }

    /// <summary>
    /// Draws placeholder images for any non-activated screens on the preview.
    /// Will release the specified device context handle if it needs to draw anything.
    /// </summary>
    public static void DrawPlaceholders(
        Graphics graphics, BoxStyle screenStyle, IList<BoxBounds> screenBounds)
    {
        // we can exclude the activated screen because we've already draw
        // the screen capture image for that one on the preview
        if (screenBounds.Any())
        {
            var brush = new SolidBrush(screenStyle.BackgroundStyle.Color1);
            graphics.FillRectangles(brush, screenBounds.Select(bounds => bounds.PaddingBounds.ToRectangle()).ToArray());
        }
    }

    /// <summary>
    /// Draws a screen capture from the specified desktop handle onto the target device context.
    /// </summary>
    public static void DrawScreenshot(
        HDC sourceHdc,
        HDC targetHdc,
        RectangleInfo sourceBounds,
        BoxBounds targetBounds)
    {
        var source = sourceBounds.ToRectangle();
        var target = targetBounds.ContentBounds.ToRectangle();
        var result = Gdi32.StretchBlt(
            targetHdc,
            target.X,
            target.Y,
            target.Width,
            target.Height,
            sourceHdc,
            source.X,
            source.Y,
            source.Width,
            source.Height,
            Gdi32.ROP_CODE.SRCCOPY);
        if (!result)
        {
            throw new InvalidOperationException(
                $"{nameof(Gdi32.StretchBlt)} returned {result.Value}");
        }
    }
}
