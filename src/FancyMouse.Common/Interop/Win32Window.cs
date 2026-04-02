using Windows.Win32.Foundation;

namespace FancyMouse.Common.Interop;

public sealed class Win32Window
{
    internal Win32Window(Win32WindowClass windowClass, string windowName, HWND hwnd)
    {
        this.WindowClass = windowClass ?? throw new ArgumentNullException(nameof(windowClass));
        this.WindowName = windowName ?? throw new ArgumentNullException(nameof(windowName));
        this.Hwnd = hwnd;
    }

    public Win32WindowClass WindowClass
    {
        get;
    }

    public string WindowName
    {
        get;
    }

    public nint Hwnd
    {
        get;
    }
}
