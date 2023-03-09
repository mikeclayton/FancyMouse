using System.Drawing.Imaging;

namespace FancyMouse.PerfTests.Drawing;

internal static class GraphicsScreenCopyHelper
{
    /// <summary>
    /// Takes a copy of the rectangle that contains the entire desktop, including areas
    /// outside the screen regions. The resulting image is a 1:1 scale copy of the
    /// current state of the desktop.
    /// </summary>
    public static Bitmap CopyFromScreen(
        Rectangle desktopBounds)
    {
        // take a screenshot of the entire desktop
        // see https://learn.microsoft.com/en-gb/windows/win32/gdi/the-virtual-screen
        var screenshot = new Bitmap(desktopBounds.Width, desktopBounds.Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(screenshot);

        // note - it *might* be faster to capture each screen individually and assemble
        // them into a single image as we *may* not have to transfer all of the blank
        // areas that are outside the screen regions - e.g. the *** in the ascii art below
        //
        // +----------------+********
        // |                |********
        // |       1        +-------+
        // |                |       |
        // +----------------+   0   |
        // *****************|       |
        // *****************+-------+
        //
        // for very irregular monitor layouts this *might* be a big percentage of the
        // rectangle containing the desktop bounds.
        graphics.CopyFromScreen(desktopBounds.Left, desktopBounds.Top, 0, 0, desktopBounds.Size);

        return screenshot;
    }
}
