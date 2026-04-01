namespace FancyMouse.PlatformServices.Abstractions;

public interface IPlatformServices
{
    public IMouseProvider Mouse
    {
        get;
    }
}
