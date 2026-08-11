public static Win32Result<BOOL> EnumDisplayMonitors(HDC hdc, RECT? lprcClip, MONITORENUMPROC lpfnEnum, LPARAM dwData)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaymonitors
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    return PInvoke.EnumDisplayMonitors(hdc, lprcClip, lpfnEnum, dwData)
        .SuccessIsNonZero();
}
