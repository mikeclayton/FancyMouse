using System.Drawing;

using FancyMouse.Models.Layout;

namespace FancyMouse.Common.Capture;

/// <summary>
/// Receives screenshots as <see cref="ScreenshotCapturePipeline"/> captures them, one screen at
/// a time, in whatever order they actually complete.
/// </summary>
public interface IScreenshotCaptureSink
{
    /// <summary>
    /// Applies <paramref name="bitmap"/> as the screenshot for <paramref name="screenLayout"/>.
    /// The caller retains ownership of <paramref name="bitmap"/> and disposes it once this
    /// returns, so implementations that need to keep the pixel data must copy it out before
    /// returning rather than holding onto the bitmap itself.
    /// </summary>
    Task SetScreenshotAsync(ScreenLayout screenLayout, Bitmap bitmap);
}
