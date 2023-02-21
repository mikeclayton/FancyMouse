namespace FancyMouse.PerfTests.ScreenCopying;

public interface IScreenCopyHelper
{

    Bitmap CopyFromScreen(
        Rectangle desktopBounds, IEnumerable<Rectangle> desktopRegions, Size screenshotSize
    );

}
