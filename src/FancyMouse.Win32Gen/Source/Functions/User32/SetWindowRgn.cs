internal static Win32Result<int> SetWindowRgn(HWND hWnd, HRGN hRgn, BOOL bRedraw)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowrgn
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    return PInvoke.SetWindowRgn(hWnd, hRgn, bRedraw)
        .SuccessIsNonZero();
}
