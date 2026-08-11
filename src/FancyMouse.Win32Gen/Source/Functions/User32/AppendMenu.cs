public static Win32Result<BOOL> AppendMenu(SafeHandle hMenu, MENU_ITEM_FLAGS uFlags, nuint uIDNewItem, string? lpNewItem)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-appendmenuw
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.AppendMenu(hMenu, uFlags, uIDNewItem, lpNewItem)
        .SuccessIsNonZero()
        .WithLastError();
}
