using FancyMouse.PlatformServices.Abstractions;

namespace FancyMouse.PlatformServices.Windows;

public sealed class WindowsPlatformServices : IPlatformServices
{
    public WindowsPlatformServices()
    {
        this.LazyMouse = new Lazy<IMouseProvider>(() => new WindowsMouseProvider());
    }

    private Lazy<IMouseProvider> LazyMouse
    {
        get;
    }

    public IMouseProvider Mouse
    {
        get => this.LazyMouse.Value;
    }
}
