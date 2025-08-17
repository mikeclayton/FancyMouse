using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

public static partial class Shell32
{
    /// <summary>
    /// Flags that either indicate which of the other members of the structure contain valid data
    /// or provide additional information to the tooltip as to how it should display.
    /// </summary>
    /// <remarks>>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataw
    /// </remarks>
    [Flags]
    internal enum NOTIFY_ICON_DATA_FLAGS : uint
    {
        NIF_MESSAGE = 0x00000001,
        NIF_ICON = 0x00000002,
        NIF_TIP = 0x00000004,
        NIF_STATE = 0x00000008,
        NIF_INFO = 0x00000010,
        NIF_GUID = 0x00000020,
        NIF_REALTIME = 0x00000040,
        NIF_SHOWTIP = 0x00000080,
    }
}
