public static Win32Result<BOOL> SetForegroundWindow(HWND hWnd)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow
    // If the window was brought to the foreground, the return value is nonzero.
    // If the window was not brought to the foreground, the return value is zero.
    return PInvoke.SetForegroundWindow(hWnd)
        .SuccessIsNonZero();
}
