using FancyMouse.Models.Drawing;

namespace FancyMouse.PlatformServices.Abstractions;

public interface IMouseProvider
{
    public PointInfo GetCursorPosition();

    public void SetCursorPosition(PointInfo position);
}
