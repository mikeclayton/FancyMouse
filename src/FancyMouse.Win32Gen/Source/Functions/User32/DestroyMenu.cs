internal static Win32Result<BOOL> DestroyMenu(HMENU hMenu)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroymenu
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.DestroyMenu(hMenu)
        .SuccessIsNonZero()
        .WithLastError();
}
