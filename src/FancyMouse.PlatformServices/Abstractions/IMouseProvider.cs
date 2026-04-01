using FancyMouse.PlatformServices.Models;

namespace FancyMouse.PlatformServices.Abstractions;

public interface IMouseProvider
{
    public Point GetCursorPosition();

    public void SetCursorPosition(Point position);
}
