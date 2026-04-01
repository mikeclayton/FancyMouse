using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;

namespace FancyMouse.PlatformServices.Abstractions;

public interface IScreenProvider
{
    IEnumerable<ScreenInfo> GetAllScreens();

    RectangleInfo GetVirtualScreen();
}
