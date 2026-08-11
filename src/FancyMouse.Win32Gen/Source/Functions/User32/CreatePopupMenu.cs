public static Win32Result<HMENU> CreatePopupMenu()
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createpopupmenu
    // If the function succeeds, the return value is a handle to the newly created menu.
    // If the function fails, the return value is NULL.
    // To get extended error information, call GetLastError.
    return PInvoke.CreatePopupMenu()
        .SuccessIsNotNull()
        .WithLastError();
}
