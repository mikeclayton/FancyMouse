public static Win32Result<BOOL> UnregisterHotKey(HWND hWnd, int id)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterhotkey
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.UnregisterHotKey(hWnd, id)
        .SuccessIsNonZero()
        .WithLastError();
}
