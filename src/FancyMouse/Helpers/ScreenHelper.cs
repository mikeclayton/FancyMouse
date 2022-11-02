using System.Drawing.Imaging;

namespace FancyMouse.Helpers
{

    internal static class ScreenHelper
    {

        public static Rectangle GetDesktopBounds()
        {
            var desktopBounds = Rectangle.Empty;
            foreach (var screen in Screen.AllScreens)
            {
                desktopBounds = Rectangle.Union(desktopBounds, screen.Bounds);
            }
            return desktopBounds;
        }

        public static Bitmap GetDesktopImage()
        {
            var desktopBounds = ScreenHelper.GetDesktopBounds();
            var desktopImage = new Bitmap(desktopBounds.Width, desktopBounds.Height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(desktopImage);
            graphics.CopyFromScreen(desktopBounds.Left, desktopBounds.Top, 0, 0, desktopBounds.Size);
            return desktopImage;
        }

    }

}
