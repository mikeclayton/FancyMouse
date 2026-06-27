using System.Diagnostics;
using System.Drawing;

using FancyMouse.Drawing.Win32Api;
using FancyMouse.Models.Drawing;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.Drawing.Screens;

/// <summary>
/// Implements an IImageRegionCopyService that uses the current desktop window as the copy source.
/// This is used during the main application runtime to generate preview images of the desktop.
/// </summary>
public sealed class DesktopImageRegionCopyService : IImageRegionCopyService
{
    /// <summary>
    /// Copies the source region from the current desktop window
    /// to the target region on the specified Graphics object.
    /// </summary>
    public void CopyImageRegion(
        Graphics targetGraphics,
        RectangleInfo sourceBounds,
        RectangleInfo targetBounds)
    {
        var stopwatch = Stopwatch.StartNew();
        var (desktopHwnd, desktopHdc) = DesktopImageRegionCopyService.GetDesktopDeviceContext();
        var previewHdc = DesktopImageRegionCopyService.GetGraphicsDeviceContext(
            targetGraphics, STRETCH_BLT_MODE.STRETCH_HALFTONE);
        stopwatch.Stop();

        var source = sourceBounds.ToRectangle();
        var target = targetBounds.ToRectangle();
        _ = Gdi32.StretchBlt(
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
            ROP_CODE.SRCCOPY);

        // we need to release the graphics device context handle before anything
        // else tries to use the Graphics object - otherwise it'll give an error
        // from GDI saying "Object is currently in use elsewhere"
        DesktopImageRegionCopyService.FreeGraphicsDeviceContext(targetGraphics, ref previewHdc);

        DesktopImageRegionCopyService.FreeDesktopDeviceContext(ref desktopHwnd, ref desktopHdc);
    }

    private static (HWND DesktopHwnd, HDC DesktopHdc) GetDesktopDeviceContext()
    {
        var desktopHwnd = User32.GetDesktopWindow();
        var desktopHdc = User32.GetWindowDC(desktopHwnd);
        return (desktopHwnd, desktopHdc);
    }

    private static void FreeDesktopDeviceContext(ref HWND desktopHwnd, ref HDC desktopHdc)
    {
        if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
        {
            _ = User32.ReleaseDC(desktopHwnd, desktopHdc);
        }

        desktopHwnd = HWND.Null;
        desktopHdc = HDC.Null;
    }

    /// <summary>
    /// Checks if the target device context handle exists, and creates a new one from the
    /// specified Graphics object if not.
    /// </summary>
    private static HDC GetGraphicsDeviceContext(Graphics graphics, STRETCH_BLT_MODE mode)
    {
        var graphicsHdc = (HDC)graphics.GetHdc();
        _ = Gdi32.SetStretchBltMode(graphicsHdc, mode);
        return graphicsHdc;
    }

    /// <summary>
    /// Free the specified device context handle if it exists.
    /// </summary>
    private static void FreeGraphicsDeviceContext(Graphics graphics, ref HDC graphicsHdc)
    {
        if (graphicsHdc.IsNull)
        {
            return;
        }

        graphics.ReleaseHdc(graphicsHdc);
        graphicsHdc = HDC.Null;
    }
}
