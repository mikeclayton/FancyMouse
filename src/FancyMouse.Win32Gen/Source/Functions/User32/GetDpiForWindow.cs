internal static Win32Result<uint> GetDpiForWindow(HWND hwnd)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforwindow
    // The DPI for the window, which depends on the DPI_AWARENESS of the window.
    // An invalid hwnd value will result in a return value of 0.
    return PInvoke.GetDpiForWindow(hwnd)
        .SuccessIsNonZero();
}
