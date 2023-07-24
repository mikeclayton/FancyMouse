using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.NativeMethods;

internal static partial class Shell32
{
    /// <summary>
    /// A value that specifies the action to be taken by Shell_NotifyIconW
    /// </summary>
    /// <remarks>>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Names and values taken from Win32Api")]
    internal enum NOTIFY_ICON_MESSAGE : uint
    {
        NIM_ADD = 0U,
        NIM_MODIFY = 1U,
        NIM_DELETE = 2U,
        NIM_SETFOCUS = 3U,
        NIM_SETVERSION = 4U,
    }
}
