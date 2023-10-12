using System.Diagnostics;
using FancyMouse.Models.Drawing;
using FancyMouse.NativeMethods;
using static FancyMouse.NativeMethods.Core;

namespace FancyMouse.Helpers.Screenshot;

/// <summary>
/// Implements an IScreenshotProvider that uses the current desktop window as the screenshot source.
/// </summary>
internal sealed class DesktopScreenshotProvider : IScreenshotProvider
{
    /// <summary>
    /// Draws a screen capture from the current desktop window onto the image
    /// wrapped by the specified Graphics object.
    /// </summary>
    public void DrawScreenshot(
        Graphics targetGraphics,
        RectangleInfo sourceBounds,
        RectangleInfo targetBounds)
    {
        var stopwatch = Stopwatch.StartNew();
        var (desktopHwnd, desktopHdc) = DesktopScreenshotProvider.GetDesktopDeviceContext();
        var previewHdc = DesktopScreenshotProvider.GetTargetDeviceContext(targetGraphics);
        stopwatch.Stop();

        var source = sourceBounds.ToRectangle();
        var target = targetBounds.ToRectangle();
        var result = Gdi32.StretchBlt(
            previewHdc,
            target.X,
            target.Y,
            target.Width,
            target.Height,
            desktopHdc,
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

        // we need to release the graphics device context handle before anything
        // else tries to use the Graphics object otherwise it'll give an error
        // from GDI saying "Object is currently in use elsewhere"
        DesktopScreenshotProvider.FreeTargetDeviceContext(targetGraphics, ref previewHdc);

        DesktopScreenshotProvider.FreeDesktopDeviceContext(ref desktopHwnd, ref desktopHdc);
    }

    private static (HWND DesktopHwnd, HDC DesktopHdc) GetDesktopDeviceContext()
    {
        var desktopHwnd = User32.GetDesktopWindow();
        var desktopHdc = User32.GetWindowDC(desktopHwnd);
        if (desktopHdc.IsNull)
        {
            throw new InvalidOperationException(
                $"{nameof(User32.GetWindowDC)} returned null");
        }

        return (desktopHwnd, desktopHdc);
    }

    private static void FreeDesktopDeviceContext(ref HWND desktopHwnd, ref HDC desktopHdc)
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
    /// Checks if the target device context handle exists, and creates a new one from the
    /// specified Graphics object if not.
    /// </summary>
    private static HDC GetTargetDeviceContext(Graphics targetGraphics)
    {
        var targetHdc = (HDC)targetGraphics.GetHdc();

        var result = Gdi32.SetStretchBltMode(targetHdc, Gdi32.STRETCH_BLT_MODE.STRETCH_HALFTONE);
        if (result == 0)
        {
            throw new InvalidOperationException(
                $"{nameof(Gdi32.SetStretchBltMode)} returned {result}");
        }

        return targetHdc;
    }

    /// <summary>
    /// Free the specified device context handle if it exists.
    /// </summary>
    private static void FreeTargetDeviceContext(Graphics targetGraphics, ref HDC targetHdc)
    {
        if ((targetGraphics is not null) && !targetHdc.IsNull)
        {
            targetGraphics.ReleaseHdc(targetHdc.Value);
            targetHdc = HDC.Null;
        }
    }
}
