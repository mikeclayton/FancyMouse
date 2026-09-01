internal static Win32Result<BOOL> ShowWindow(HWND hWnd, SHOW_WINDOW_CMD nCmdShow)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
    // If the window was previously visible, the return value is nonzero.
    // If the window was previously hidden, the return value is zero.
    return PInvoke.ShowWindow(hWnd, nCmdShow)
        .AlwaysSucceeds();
}
