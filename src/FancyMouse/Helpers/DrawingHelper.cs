using System.Drawing.Drawing2D;
using FancyMouse.Drawing.Models;
using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.Helpers;

internal static class DrawingHelper
{
    public static LayoutInfo CalculateLayoutInfo(
        LayoutConfig layoutConfig)
    {
        if (layoutConfig is null)
        {
            throw new ArgumentNullException(nameof(layoutConfig));
        }

        var builder = new LayoutInfo.Builder
        {
            LayoutConfig = layoutConfig,
        };

        builder.ActivatedScreen = layoutConfig.ScreenBounds[layoutConfig.ActivatedScreen];

        // work out the maximum *constrained* form size
        // * can't be bigger than the activated screen
        // * can't be bigger than the max form size
        var maxFormSize = builder.ActivatedScreen.Size
            .Intersect(layoutConfig.MaximumFormSize);

        // the drawing area for screen images is inside the
        // form border and inside the preview border
        var maxDrawingSize = maxFormSize
            .Shrink(layoutConfig.FormPadding)
            .Shrink(layoutConfig.PreviewPadding);

        // scale the virtual screen to fit inside the drawing bounds
        var scalingRatio = layoutConfig.VirtualScreen.Size
            .ScaleToFitRatio(maxDrawingSize);

        // position the drawing bounds inside the preview border
        var drawingBounds = layoutConfig.VirtualScreen.Size
            .ScaleToFit(maxDrawingSize)
            .PlaceAt(layoutConfig.PreviewPadding.Left, layoutConfig.PreviewPadding.Top);

        // now we know the size of the drawing area we can work out the preview size
        builder.PreviewBounds = drawingBounds.Enlarge(layoutConfig.PreviewPadding);

        // ... and the form size
        // * center the form to the activated position, but nudge it back
        //   inside the visible area of the activated screen if it falls outside
        builder.FormBounds = builder.PreviewBounds.Size
            .PlaceAt(0, 0)
            .Enlarge(layoutConfig.FormPadding)
            .Center(layoutConfig.ActivatedLocation)
            .Clamp(builder.ActivatedScreen);

        // now calculate the positions of each of the screen images on the preview
        builder.ScreenBounds = layoutConfig.ScreenBounds
            .Select(
                screen => screen
                    .Offset(layoutConfig.VirtualScreen.Location.Size.Negate())
                    .Scale(scalingRatio)
                    .Offset(layoutConfig.PreviewPadding.Left, layoutConfig.PreviewPadding.Top))
            .ToList();

        return builder.Build();
    }

    /// <summary>
    /// Resize and position the specified form.
    /// </summary>
    public static void PositionForm(
        Form form, RectangleInfo formBounds)
    {
        // note - do this in two steps rather than "this.Bounds = formBounds" as there
        // appears to be an issue in WinForms with dpi scaling even when using PerMonitorV2,
        // where the form scaling uses either the *primary* screen scaling or the *previous*
        // screen's scaling when the form is moved to a different screen. i've got no idea
        // *why*, but the exact sequence of calls below seems to be a workaround...
        // see https://github.com/mikeclayton/FancyMouse/issues/2
        var bounds = formBounds.ToRectangle();
        form.Location = bounds.Location;
        _ = form.PointToScreen(Point.Empty);
        form.Size = bounds.Size;
    }

    /// <summary>
    /// Draw the preview background.
    /// </summary>
    public static void DrawPreviewBackground(
        Graphics previewGraphics, RectangleInfo previewBounds, IEnumerable<RectangleInfo> screenBounds)
    {
        using var backgroundBrush = new LinearGradientBrush(
            previewBounds.Location.ToPoint(),
            previewBounds.Size.ToPoint(),
            Color.FromArgb(13, 87, 210), // light blue
            Color.FromArgb(3, 68, 192)); // darker blue

        // it's faster to build a region with the screen areas excluded
        // and fill that than it is to fill the entire bounding rectangle
        var backgroundRegion = new Region(previewBounds.ToRectangle());
        foreach (var screen in screenBounds)
        {
            backgroundRegion.Exclude(screen.ToRectangle());
        }

        previewGraphics.FillRegion(backgroundBrush, backgroundRegion);
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
    /// Graphics object if not.
    /// </summary>
    public static void EnsurePreviewDeviceContext(Graphics previewGraphics, ref HDC previewHdc)
    {
        if (previewHdc.IsNull)
        {
            previewHdc = new HDC(previewGraphics.GetHdc());
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
    public static void FreePreviewDeviceContext(Graphics previewGraphics, ref HDC previewHdc)
    {
        if ((previewGraphics is not null) && !previewHdc.IsNull)
        {
            previewGraphics.ReleaseHdc(previewHdc.Value);
            previewHdc = HDC.Null;
        }
    }

    /// <summary>
    /// Draw placeholder images for any non-activated screens on the preview.
    /// Will release the specified device context handle if it needs to draw anything.
    /// </summary>
    public static void DrawPreviewPlaceholders(
        Graphics previewGraphics, IEnumerable<RectangleInfo> screenBounds)
    {
        // we can exclude the activated screen because we've already draw
        // the screen capture image for that one on the preview
        if (screenBounds.Any())
        {
            var brush = Brushes.Black;
            previewGraphics.FillRectangles(brush, screenBounds.Select(screen => screen.ToRectangle()).ToArray());
        }
    }

    /// <summary>
    /// Draw placeholder images for any non-activated screens on the preview.
    /// Will release the specified device context handle if it needs to draw anything.
    /// </summary>
    public static void DrawPreviewPlaceholders2(
        HDC previewHdc, IEnumerable<RectangleInfo> screenBounds)
    {
        // we can exclude the activated screen because we've already draw
        // the screen capture image for that one on the preview
        var brush = Gdi32.CreateSolidBrush(
            new Gdi32.COLORREF(0, 0, 0));
        if (brush.IsNull)
        {
            throw new InvalidOperationException(
                $"{nameof(Gdi32.CreateSolidBrush)} returned {brush}");
        }

        foreach (var screen in screenBounds)
        {
            var target = new RECT(
                left: (int)screen.X,
                top: (int)screen.Y,
                right: (int)screen.Right,
                bottom: (int)screen.Bottom);
            var result = User32.FillRect(
                previewHdc,
                ref target,
                brush);
            if (result == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(Gdi32.SetStretchBltMode)} returned {result}");
            }
        }
    }

    /// <summary>
    /// Draws screen captures from the specified desktop handle onto the target device context.
    /// </summary>
    public static void DrawPreviewScreen(
        HDC sourceHdc,
        HDC targetHdc,
        RectangleInfo sourceBounds,
        RectangleInfo targetBounds)
    {
        var source = sourceBounds.ToRectangle();
        var target = targetBounds.ToRectangle();
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

    /// <summary>
    /// Draws screen captures from the specified desktop handle onto the target device context.
    /// </summary>
    public static void DrawPreviewScreens(
        HDC sourceHdc,
        HDC targetHdc,
        IList<RectangleInfo> sourceBounds,
        IList<RectangleInfo> targetBounds)
    {
        for (var i = 0; i < sourceBounds.Count; i++)
        {
            var source = sourceBounds[i].ToRectangle();
            var target = targetBounds[i].ToRectangle();
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
}
