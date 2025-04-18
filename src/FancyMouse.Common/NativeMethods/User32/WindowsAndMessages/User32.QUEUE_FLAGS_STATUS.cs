using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getqueuestatus
    /// </remarks>
    [Flags]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names and values taken from Win32Api")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Names and values taken from Win32Api")]
    internal enum QUEUE_STATUS_FLAGS : uint
    {
        QS_ALLEVENTS = 0x000004BF,
        QS_ALLINPUT = 0x000004FF,
        QS_ALLPOSTMESSAGE = 0x00000100,
        QS_HOTKEY = 0x00000080,
        QS_INPUT = 0x00000407,
        QS_KEY = 0x00000001,
        QS_MOUSE = 0x00000006,
        QS_MOUSEBUTTON = 0x00000004,
        QS_MOUSEMOVE = 0x00000002,
        QS_PAINT = 0x00000020,
        QS_POSTMESSAGE = 0x00000008,
        QS_RAWINPUT = 0x00000400,
        QS_SENDMESSAGE = 0x00000040,
        QS_TIMER = 0x00000010,
    }
}
