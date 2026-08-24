using System.Drawing;
using System.Drawing.Drawing2D;

namespace FancyMouse.Common.Bezels;

internal static class GraphicsHelpers
{
    public static void EnableAntialias(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;
    }
}
