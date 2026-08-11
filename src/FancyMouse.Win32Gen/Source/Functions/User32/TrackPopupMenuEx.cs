public static Win32Result<BOOL> TrackPopupMenuEx(SafeHandle hMenu, uint uFlags, int x, int y, HWND hwnd, TPMPARAMS? lptpm)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-trackpopupmenuex
    // If you specify TPM_RETURNCMD in the fuFlags parameter, the return value is the menu-item identifier of the item that the user selected.
    // If the user cancels the menu without making a selection, or if an error occurs, the return value is zero.
    // If you do not specify TPM_RETURNCMD in the fuFlags parameter, the return value is nonzero if the function succeeds and zero if it fails.
    // To get extended error information, call GetLastError.
    var result = PInvoke.TrackPopupMenuEx(hMenu, uFlags, x, y, hwnd, lptpm)
        .SuccessIsNonZero();

    // With TPM_RETURNCMD set, a zero result means either "the user cancelled"
    // or "an error occurred" - the docs don't say GetLastError can tell those
    // apart, so it isn't captured here, to avoid reporting a stale or
    // misleading error code for what may just be a cancelled selection.
    if ((uFlags & (uint)TRACK_POPUP_MENU_FLAGS.TPM_RETURNCMD) == 0)
    {
        result = result.WithLastError();
    }

    return result;
}
