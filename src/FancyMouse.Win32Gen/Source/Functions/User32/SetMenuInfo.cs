internal static Win32Result<BOOL> SetMenuInfo(SafeHandle param0, in MENUINFO param1)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setmenuinfo
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.SetMenuInfo(param0, param1)
        .SuccessIsNonZero()
        .WithLastError();
}
