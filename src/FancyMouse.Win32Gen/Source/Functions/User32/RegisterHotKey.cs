internal static Win32Result<BOOL> RegisterHotKey(HWND hWnd, int id, HOT_KEY_MODIFIERS fsModifiers, uint vk)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.RegisterHotKey(hWnd, id, fsModifiers, vk)
        .SuccessIsNonZero()
        .WithLastError();
}
