internal static Win32Result<LRESULT> DefWindowProc(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-defwindowprocw
    // The return value is the result of the message processing and depends on the message.
    return PInvoke.DefWindowProc(hWnd, Msg, wParam, lParam)
        .AlwaysSucceeds();
}
