namespace FancyMouse.Win32.NativeMethods;

public static partial class Shell32
{
    /// <summary>
    /// The state of the icon.
    /// </summary>
    /// <remarks>>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/shellapi/ns-shellapi-notifyicondataw
    /// </remarks>
    public enum NOTIFY_ICON_STATE : uint
    {
        NIS_HIDDEN = 0x00000001,
        NIS_SHAREDICON = 0x00000002,
    }
}
