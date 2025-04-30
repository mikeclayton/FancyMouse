namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-trackpopupmenuex
    /// </remarks>
    [Flags]
    internal enum TRACK_POPUP_MENU_FLAGS : uint
    {
        /// <summary>
        /// Centers the shortcut menu horizontally relative to the coordinate specified by the x parameter.
        /// </summary>
        TPM_CENTERALIGN = 0x0004,

        /// <summary>
        /// Positions the shortcut menu so that its left side is aligned with the coordinate specified by the x parameter.
        /// </summary>
        TPM_LEFTALIGN = 0x0000,

        /// <summary>
        /// Positions the shortcut menu so that its right side is aligned with the coordinate specified by the x parameter.
        /// </summary>
        TPM_RIGHTALIGN = 0x0008,

        /// <summary>
        /// Positions the shortcut menu so that its bottom side is aligned with the coordinate specified by the y parameter.
        /// </summary>
        TPM_BOTTOMALIGN = 0x0020,

        /// <summary>
        /// Positions the shortcut menu so that its top side is aligned with the coordinate specified by the y parameter.
        /// </summary>
#pragma warning disable CA1069 // Enums values should not be duplicated
        TPM_TOPALIGN = 0x0000,
#pragma warning restore CA1069 // Enums values should not be duplicated

        /// <summary>
        /// Centers the shortcut menu vertically relative to the coordinate specified by the y parameter.
        /// </summary>
        TPM_VCENTERALIGN = 0x0010,

        /// <summary>
        /// The function does not send notification messages when the user clicks a menu item.
        /// </summary>
        TPM_NONOTIFY = 0x0080,

        /// <summary>
        /// The function returns the menu item identifier of the user's selection in the return value.
        /// </summary>
        TPM_RETURNCMD = 0x0100,

        /// <summary>
        /// The user can select menu items with only the left mouse button.
        /// </summary>
#pragma warning disable CA1069 // Enums values should not be duplicated
        TPM_LEFTBUTTON = 0x0000,
#pragma warning restore CA1069 // Enums values should not be duplicated

        /// <summary>
        /// The user can select menu items with both the left and right mouse buttons.
        /// </summary>
        TPM_RIGHTBUTTON = 0x0002,

        /// <summary>
        /// Animates the menu from right to left.
        /// </summary>
        TPM_HORNEGANIMATION = 0x0800,

        /// <summary>
        /// Animates the menu from left to right.
        /// </summary>
        TPM_HORPOSANIMATION = 0x0400,

        /// <summary>
        /// Displays menu without animation.
        /// </summary>
        TPM_NOANIMATION = 0x4000,

        /// <summary>
        /// Animates the menu from bottom to top.
        /// </summary>
        TPM_VERNEGANIMATION = 0x2000,

        /// <summary>
        /// Animates the menu from top to bottom.
        /// </summary>
        TPM_VERPOSANIMATION = 0x1000,
    }
}
