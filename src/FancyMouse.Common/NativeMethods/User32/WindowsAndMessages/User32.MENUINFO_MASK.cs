namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-menuinfo
    /// </summary>
    [Flags]
    internal enum MENUINFO_MASK : uint
    {
        /// <summary>
        /// Settings apply to the menu and all of its submenus. SetMenuInfo uses this flag and GetMenuInfo ignores this flag
        /// </summary>
        MIM_APPLYTOSUBMENUS = 0x80000000,

        /// <summary>
        /// Retrieves or sets the hbrBack member.
        /// </summary>
        MIM_BACKGROUND = 0x00000002,

        /// <summary>
        /// Retrieves or sets the dwContextHelpID member.
        /// </summary>
        MIM_HELPID = 0x00000004,

        /// <summary>
        /// Retrieves or sets the cyMax member.
        /// </summary>
        MIM_MAXHEIGHT = 0x00000001,

        /// <summary>
        /// Retrieves or sets the dwMenuData member.
        /// </summary>
        MIM_MENUDATA = 0x00000008,

        /// <summary>
        /// Retrieves or sets the dwStyle member.
        /// </summary>
        MIM_STYLE = 0x00000010,
    }
}
