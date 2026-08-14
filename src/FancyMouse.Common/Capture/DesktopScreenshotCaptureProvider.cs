using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

using FancyMouse.Common.Win32Gen;
using FancyMouse.Models.Drawing;

using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.Common.Capture;

/// <summary>
/// Implements an <see cref="IScreenshotCaptureProvider"/> that captures from the current
/// interactive desktop using <c>StretchBlt</c>. This is used during the main application
/// runtime to generate preview images of the desktop.
/// </summary>
/// <remarks>
/// EXPERIMENTAL - a single instance of this provider is shared across every screen on the same
/// device. It used to serialize every <see cref="CaptureAsync"/> call through a lock, on the
/// assumption that the underlying GDI device contexts could only service one <c>StretchBlt</c>
/// call at a time - that serialization is temporarily removed to test whether that assumption
/// actually holds, since each call already acquires its own independent desktop device context
/// (see <see cref="GetDesktopDeviceContext"/>) rather than sharing one, so this is testing
/// genuinely concurrent <c>StretchBlt</c> calls against independent handles, not concurrent use
/// of a shared handle. If captures come back corrupted or GDI errors show up, that's the
/// answer, and the lock needs putting back.
/// </remarks>
public sealed class DesktopScreenshotCaptureProvider : IScreenshotCaptureProvider, IDisposable
{
    // TEMPORARY - diagnosing where per-screen capture time actually goes. Remove this
    // constructor parameter and every diagnosticLogger call alongside it once that's understood.
    private readonly Action<string>? diagnosticLogger;

    public DesktopScreenshotCaptureProvider(Action<string>? diagnosticLogger = null)
    {
        this.diagnosticLogger = diagnosticLogger;
    }

    public void Dispose()
    {
        // nothing to release here while the capture lock is out - see the class remarks
    }

    public async Task<Bitmap> CaptureAsync(
        RectangleInfo sourceArea,
        SizeInfo thumbnailSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dispatchStopwatch = Stopwatch.StartNew();
        var result = await Task.Run(
            () => DesktopScreenshotCaptureProvider.Capture(sourceArea, thumbnailSize, this.diagnosticLogger),
            cancellationToken)
            .ConfigureAwait(false);
        dispatchStopwatch.Stop();

        // TEMPORARY - see diagnosticLogger remarks above
        this.diagnosticLogger?.Invoke(
            $"DIAG capture {sourceArea} -> {thumbnailSize}: " +
            $"dispatchToDone={dispatchStopwatch.ElapsedMilliseconds}ms (includes Task.Run hop)");

        return result;
    }

    private static Bitmap Capture(
        RectangleInfo sourceArea,
        SizeInfo thumbnailSize,
        Action<string>? diagnosticLogger)
    {
        var setupStopwatch = Stopwatch.StartNew();

        var target = thumbnailSize.Round().ToSize();
        var thumbnailImage = new Bitmap(target.Width, target.Height, PixelFormat.Format32bppPArgb);
        using var thumbnailGraphics = Graphics.FromImage(thumbnailImage);

        var (desktopHwnd, desktopHdc) = DesktopScreenshotCaptureProvider.GetDesktopDeviceContext();
        var thumbnailHdc = DesktopScreenshotCaptureProvider.GetGraphicsDeviceContext(
            thumbnailGraphics, STRETCH_BLT_MODE.STRETCH_HALFTONE);
        setupStopwatch.Stop();

        var bltStopwatch = Stopwatch.StartNew();
        var source = sourceArea.ToRectangle();
        _ = Gdi32.StretchBlt(
            thumbnailHdc,
            0,
            0,
            target.Width,
            target.Height,
            desktopHdc,
            source.X,
            source.Y,
            source.Width,
            source.Height,
            ROP_CODE.SRCCOPY)
            .ThrowIfFailed();
        bltStopwatch.Stop();

        var cleanupStopwatch = Stopwatch.StartNew();

        // we need to release the graphics device context handle before anything
        // else tries to use the Graphics object - otherwise it'll give an error
        // from GDI saying "Object is currently in use elsewhere"
        DesktopScreenshotCaptureProvider.FreeGraphicsDeviceContext(thumbnailGraphics, ref thumbnailHdc);

        DesktopScreenshotCaptureProvider.FreeDesktopDeviceContext(ref desktopHwnd, ref desktopHdc);
        cleanupStopwatch.Stop();

        // TEMPORARY - see diagnosticLogger remarks on the constructor
        diagnosticLogger?.Invoke(
            $"DIAG capture phases for {target.Width}x{target.Height}: " +
            $"setup={setupStopwatch.ElapsedMilliseconds}ms, " +
            $"stretchBlt={bltStopwatch.ElapsedMilliseconds}ms, " +
            $"cleanup={cleanupStopwatch.ElapsedMilliseconds}ms");

        return thumbnailImage;
    }

    private static (HWND DesktopHwnd, HDC DesktopHdc) GetDesktopDeviceContext()
    {
        var desktopHwnd = User32.GetDesktopWindow().IgnoreFailure().GetValue();
        var desktopHdc = User32.GetWindowDC(desktopHwnd).ThrowIfFailed().GetValue();
        return (desktopHwnd, desktopHdc);
    }

    private static void FreeDesktopDeviceContext(ref HWND desktopHwnd, ref HDC desktopHdc)
    {
        if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
        {
            _ = User32.ReleaseDC(desktopHwnd, desktopHdc)
                .ThrowIfFailed();
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
        _ = Gdi32.SetStretchBltMode(graphicsHdc, mode)
            .ThrowIfFailed();
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
