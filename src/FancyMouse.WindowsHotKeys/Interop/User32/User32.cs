using System.Diagnostics.CodeAnalysis;

// ReSharper disable CheckNamespace
namespace FancyMouse.WindowsHotKeys.Interop;
// ReSharper restore CheckNamespace

internal static partial class User32
{

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public const int HWND_MESSAGE = -3;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public const uint CW_USEDEFAULT = 0x80000000;

}
