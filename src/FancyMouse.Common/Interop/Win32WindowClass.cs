using Windows.Win32.Foundation;

namespace FancyMouse.Common.Interop;

/// <summary>
/// Wrapper around a Windows window class that can be passed to
/// external assemblies to manage WNDPROC delegate lifetime without
/// exposing internals.
/// </summary>
internal sealed class Win32WindowClass
{
    internal Win32WindowClass(HINSTANCE hInstance, ushort classAtom, string className, Win32WindowProc windowProc)
    {
        this.HInstance = hInstance;
        this.ClassAtom = classAtom;
        this.ClassName = className ?? throw new ArgumentNullException(nameof(className));
        this.WindowProc = windowProc ?? throw new ArgumentNullException(nameof(windowProc));
    }

    /// <summary>
    /// Gets the module instance the class was registered with - windows created
    /// from this class need to pass the same value to CreateWindowEx.
    /// </summary>
    internal HINSTANCE HInstance
    {
        get;
    }

    private ushort ClassAtom
    {
        get;
    }

    public string ClassName
    {
        get;
    }

    private Win32WindowProc WindowProc
    {
        get;
    }
}
