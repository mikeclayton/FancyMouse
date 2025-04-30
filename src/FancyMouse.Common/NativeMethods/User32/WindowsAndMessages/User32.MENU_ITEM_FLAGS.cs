namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-appendmenuw
    /// </summary>
    [Flags]
    internal enum MENU_ITEM_FLAGS : uint
    {
        MF_BITMAP = 0x00000004,
        MF_CHECKED = 0x00000008,
        MF_DISABLED = 0x00000002,
        MF_ENABLED = 0x00000000,
        MF_GRAYED = 0x00000001,
        MF_MENUBARBREAK = 0x00000020,
        MF_MENUBREAK = 0x00000040,
        MF_OWNERDRAW = 0x00000100,
        MF_POPUP = 0x00000010,
        MF_SEPARATOR = 0x00000800,
#pragma warning disable CA1069 // Enums values should not be duplicated
        MF_STRING = 0x00000000,
        MF_UNCHECKED = 0x00000000,
#pragma warning restore CA1069 // Enums values should not be duplicated
    }
}
