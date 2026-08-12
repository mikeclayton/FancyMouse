internal static unsafe Win32Result<HWND> CreateWindowEx(
    WINDOW_EX_STYLE dwExStyle,
    string? lpClassName,
    string? lpWindowName,
    WINDOW_STYLE dwStyle,
    int X,
    int Y,
    int nWidth,
    int nHeight,
    HWND hWndParent,
    SafeHandle? hMenu,
    SafeHandle? hInstance,
    void* lpParam)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createwindowexw
    // If the function succeeds, the return value is a handle to the new window.
    // If the function fails, the return value is NULL.
    // To get extended error information, call GetLastError.
    return PInvoke.CreateWindowEx(dwExStyle, lpClassName, lpWindowName, dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam)
        .SuccessIsNotNull()
        .WithLastError();
}
