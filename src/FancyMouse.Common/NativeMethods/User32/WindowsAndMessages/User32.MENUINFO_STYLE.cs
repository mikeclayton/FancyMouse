namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-menuinfo
    /// </summary>
    [Flags]
    internal enum MENUINFO_STYLE : uint
    {
        /// <summary>
        /// Menu automatically ends when mouse is outside the menu for approximately 10 seconds.
        /// </summary>
        MNS_AUTODISMISS = 0x10000000,

        /// <summary>
        /// The same space is reserved for the check mark and the bitmap. If the check mark is drawn,
        /// the bitmap is not. All checkmarks and bitmaps are aligned. Used for menus where some items
        /// use checkmarks and some use bitmaps.
        /// </summary>
        MNS_CHECKORBMP = 0x04000000,

        /// <summary>
        /// Menu items are OLE drop targets or drag sources. Menu owner receives WM_MENUDRAG and
        /// WM_MENUGETOBJECT messages.
        /// </summary>
        MNS_DRAGDROP = 0x20000000,

        /// <summary>
        /// Menu is modeless; that is, there is no menu modal message loop while the menu is active.
        /// </summary>
        MNS_MODELESS = 0x40000000,

        /// <summary>
        /// No space is reserved to the left of an item for a check mark. The item can still be
        /// selected, but the check mark will not appear next to the item.
        /// </summary>
        MNS_NOCHECK = 0x80000000,

        /// <summary>
        /// Menu owner receives a WM_MENUCOMMAND message instead of a WM_COMMAND message when th
        /// e user makes a selection. MNS_NOTIFYBYPOS is a menu header style and has no effect
        /// when applied to individual sub menus.
        /// </summary>
        MNS_NOTIFYBYPOS = 0x08000000,
    }
}
