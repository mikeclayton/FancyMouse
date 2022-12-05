using FancyMouse.Lib;
using System.Drawing.Imaging;

namespace FancyMouse.Helpers
{

    internal static class ScreenHelper
    {

        public static Bitmap GetDesktopImage()
        {
            var desktopBounds = LayoutHelper.CombineBounds(
                Screen.AllScreens.Select(screen => screen.Bounds)
            );
            var desktopImage = new Bitmap(desktopBounds.Width, desktopBounds.Height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(desktopImage);
            graphics.CopyFromScreen(desktopBounds.Left, desktopBounds.Top, 0, 0, desktopBounds.Size);
            return desktopImage;
        }

    }

}
