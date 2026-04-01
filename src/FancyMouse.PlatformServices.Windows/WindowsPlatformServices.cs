using FancyMouse.PlatformServices.Abstractions;

namespace FancyMouse.PlatformServices.Windows;

public sealed class WindowsPlatformServices : IPlatformServices
{
    public WindowsPlatformServices()
    {
        this.LazyMouse = new(() => new WindowsMouseProvider());
        this.LazyScreens = new(() => new WindowsScreenProvider());
    }

    private Lazy<IMouseProvider> LazyMouse
    {
        get;
    }

    public IMouseProvider Mouse
    {
        get => this.LazyMouse.Value;
    }

    private Lazy<IScreenProvider> LazyScreens
    {
        get;
    }

    public IScreenProvider Screens
    {
        get => this.LazyScreens.Value;
    }
}
