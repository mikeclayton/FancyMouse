public static Win32Result<BOOL> PostMessage(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-postmessagew
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.PostMessage(hWnd, Msg, wParam, lParam)
        .SuccessIsNonZero()
        .WithLastError();
}
