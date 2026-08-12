internal static Win32Result<BOOL> ScreenToClient(HWND hWnd, ref Point lpPoint)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-screentoclient
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    return PInvoke.ScreenToClient(hWnd, ref lpPoint)
        .SuccessIsNonZero();
}
