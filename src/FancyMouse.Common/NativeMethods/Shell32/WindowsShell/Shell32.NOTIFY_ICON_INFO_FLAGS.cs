using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

public static partial class Shell32
{
    /// <summary>
    /// Flags that can be set to modify the behavior and appearance of a balloon notification.
    /// </summary>
    /// <remarks>>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataw
    /// </remarks>
    [Flags]
    internal enum NOTIFY_ICON_INFOTIP_FLAGS : uint
    {
        NIIF_NONE = 0x00000000,
        NIIF_INFO = 0x00000001,
        NIIF_WARNING = 0x00000002,
        NIIF_ERROR = 0x00000003,
        NIIF_USER = 0x00000004,
        NIIF_ICON_MASK = 0x0000000F,
        NIIF_NOSOUND = 0x00000010,
        NIIF_LARGE_ICON = 0x00000020,
        NIIF_RESPECT_QUIET_TIME = 0x00000080,
    }
}
