using System.Drawing.Drawing2D;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.Helpers;

internal static class DrawingHelper
{
    /// <summary>
    /// Draw a border shape.
    /// </summary>
    public static void DrawBoxBorder(
        Graphics graphics, BoxStyle boxStyle, BoxBounds boxBounds)
    {
        var borderInfo = boxStyle.BorderInfo;
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
    /// Draw a gradient-filled background shape.
    /// </summary>
    public static void DrawBoxBackground(
        Graphics graphics, BoxStyle boxStyle, BoxBounds boxBounds, IEnumerable<RectangleInfo> excludeBounds)
    {
        var backgroundBounds = boxBounds.PaddingBounds;
        var backgroundInfo = boxStyle.BackgroundInfo;

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
            previewHdc = new HDC(graphics.GetHdc());
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
    /// Draw placeholder images for any non-activated screens on the preview.
    /// Will release the specified device context handle if it needs to draw anything.
    /// </summary>
    public static void DrawPlaceholders(
        Graphics graphics, BoxStyle screenStyle, IEnumerable<BoxBounds> screenBounds)
    {
        // we can exclude the activated screen because we've already draw
        // the screen capture image for that one on the preview
        if (screenBounds.Any())
        {
            var brush = new SolidBrush(screenStyle.BackgroundInfo.Color1);
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
