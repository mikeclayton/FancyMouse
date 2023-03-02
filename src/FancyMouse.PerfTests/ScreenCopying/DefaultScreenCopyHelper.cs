using System.Drawing.Imaging;
using FancyMouse.ScreenCopying;

namespace FancyMouse.PerfTests.ScreenCopying;

public sealed class DefaultScreenCopyHelper : ICopyFromScreen
{
    public Bitmap CopyFromScreen(
        Rectangle desktopBounds, IEnumerable<Rectangle> desktopRegions, Size screenshotSize)
    {
        var screenshot = new Bitmap(desktopBounds.Width, desktopBounds.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(screenshot))
        {
            // note - it *might* be faster to capture each monitor individually and assemble them into
            // a single image ourselves as we *may* not have to transfer all of the blank pixels
            // that are outside the desktop bounds - e.g. the *** in the ascii art below
            //
            // +----------------+********
            // |                |********
            // |       1        +-------+
            // |                |       |
            // +----------------+   0   |
            // *****************|       |
            // *****************+-------+
            //
            // for very irregular monitor layouts this *might* be a big percentage of the rectangle
            // containing the desktop bounds.
            //
            // then again, it might not make much difference at all - we'd need to do some perf tests
            graphics.CopyFromScreen(desktopBounds.Left, desktopBounds.Top, 0, 0, desktopBounds.Size);
        }

        return screenshot;
    }
}
