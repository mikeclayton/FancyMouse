namespace FancyMouse.ScreenCopying;

public interface ICopyFromScreen
{

    Bitmap CopyFromScreen(
        Rectangle desktopBounds, IEnumerable<Rectangle> desktopRegions, Size screenshotSize
    );

}
