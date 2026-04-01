namespace FancyMouse.PlatformServices.Models;

public sealed class Point
{
    public Point(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public int X
    {
        get;
    }

    public int Y
    {
        get;
    }
}
