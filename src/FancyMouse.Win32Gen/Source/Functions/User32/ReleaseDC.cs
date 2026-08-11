public static Win32Result<int> ReleaseDC(HWND hWnd, HDC hDC)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-releasedc
    // The return value indicates whether the DC was released.
    // If the DC was released, the return value is 1.
    // If the DC was not released, the return value is zero.
    return PInvoke.ReleaseDC(hWnd, hDC)
        .SuccessIsNonZero();
}
