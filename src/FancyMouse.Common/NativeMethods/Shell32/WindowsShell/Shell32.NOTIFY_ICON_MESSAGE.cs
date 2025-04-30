using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

public static partial class Shell32
{
    /// <summary>
    /// A value that specifies the action to be taken by Shell_NotifyIconW
    /// </summary>
    /// <remarks>>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names and values taken from Win32Api")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Names and values taken from Win32Api")]
    internal enum NOTIFY_ICON_MESSAGE : uint
    {
        NIM_ADD = 0x00000000,
        NIM_MODIFY = 0x00000001,
        NIM_DELETE = 0x00000002,
        NIM_SETFOCUS = 0x00000003,
        NIM_SETVERSION = 0x00000004,
    }
}
