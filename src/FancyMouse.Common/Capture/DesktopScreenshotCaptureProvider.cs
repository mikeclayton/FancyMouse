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
/// A single instance of this provider is shared across every screen on the same device, but
/// the underlying GDI device contexts this uses can only service one <c>StretchBlt</c> call at
/// a time - concurrent <see cref="CaptureAsync"/> calls are accepted (each returns a pending
/// <see cref="Task{Bitmap}"/> immediately) but are then serialized internally via
/// <see cref="captureLock"/>, so callers don't need to know or care about this limitation.
/// </remarks>
public sealed class DesktopScreenshotCaptureProvider : IScreenshotCaptureProvider, IDisposable
{
    private readonly SemaphoreSlim captureLock = new(1, 1);

    // TEMPORARY - diagnosing where per-screen capture time actually goes. Remove this
    // constructor parameter and every diagnosticLogger call alongside it once that's understood.
    private readonly Action<string>? diagnosticLogger;

    public DesktopScreenshotCaptureProvider(Action<string>? diagnosticLogger = null)
    {
        this.diagnosticLogger = diagnosticLogger;
    }

    public void Dispose()
        => this.captureLock.Dispose();

    public async Task<Bitmap> CaptureAsync(
        RectangleInfo sourceArea,
        SizeInfo thumbnailSize,
        CancellationToken cancellationToken = default)
    {
        // if this request is still queued behind another screen's capture when it's
        // cancelled (e.g. a newer activation superseded it), it's abandoned here without
        // ever running - this is the main payoff of cancellation for this provider, since
        // once StretchBlt actually starts it's a single fast call that isn't worth
        // interrupting mid-flight
        var waitStopwatch = Stopwatch.StartNew();
        await this.captureLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        waitStopwatch.Stop();
        try
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
                $"lockWait={waitStopwatch.ElapsedMilliseconds}ms, " +
                $"dispatchToDone={dispatchStopwatch.ElapsedMilliseconds}ms (includes Task.Run hop)");

            return result;
        }
        finally
        {
            this.captureLock.Release();
        }
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
