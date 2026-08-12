internal static Win32Result<HWND> GetDesktopWindow()
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdesktopwindow
    // The return value is a handle to the desktop window.
    return PInvoke.GetDesktopWindow()
        .AlwaysSucceeds();
}
