using System.Drawing;
using System.Drawing.Drawing2D;

namespace FancyMouse.Drawing.ContouredBezels;

internal static class GraphicsHelpers
{
    public static void EnableAntialias(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;
    }
}
