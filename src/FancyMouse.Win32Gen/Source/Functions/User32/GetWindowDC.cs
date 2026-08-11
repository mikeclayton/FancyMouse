public static Win32Result<HDC> GetWindowDC(HWND hWnd)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowdc
    // If the function succeeds, the return value is a handle to a device context for the specified window.
    // If the function fails, the return value is NULL, indicating an error or an invalid hWnd parameter.
    return PInvoke.GetWindowDC(hWnd)
        .SuccessIsNotNull();
}
