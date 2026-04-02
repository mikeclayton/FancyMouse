using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace FancyMouse.Common.Interop;

/// <summary>
/// Wrapper around a Windows window class that can be passed to
/// external assemblies to manage WNDPROC delgate lifetime without
/// exposing internals.
/// </summary>
public sealed class Win32WindowClass
{
    internal Win32WindowClass(ushort classAtom, string className, Win32WindowProc windowProc)
    {
        this.ClassAtom = classAtom;
        this.ClassName = className ?? throw new ArgumentNullException(nameof(className));
        this.WindowProc = windowProc ?? throw new ArgumentNullException(nameof(windowProc));
    }

    internal ushort ClassAtom
    {
        get;
    }

    public string ClassName
    {
        get;
    }

    internal Win32WindowProc WindowProc
    {
        get;
    }
}
