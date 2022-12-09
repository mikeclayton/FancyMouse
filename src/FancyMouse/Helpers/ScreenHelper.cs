using FancyMouse.Lib;
using System.Drawing.Imaging;

namespace FancyMouse.Helpers
{

    internal static class ScreenHelper
    {

        public static Bitmap GetDesktopImage(Rectangle bounds)
        {
            var image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(image);
            graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
            return image;
        }

    }

}
