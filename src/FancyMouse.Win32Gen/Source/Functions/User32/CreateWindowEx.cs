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
    HMENU hMenu,
    HINSTANCE hInstance,
    void* lpParam)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-createwindowexw
    // If the function succeeds, the return value is a handle to the new window.
    // If the function fails, the return value is NULL.
    // To get extended error information, call GetLastError.
    //
    // this wraps the raw HMENU / HINSTANCE overload rather than CsWin32's
    // SafeHandle one, because the hInstance callers pass (from GetModuleHandle)
    // isn't an owned handle and there's no SafeHandle that fits it.
    fixed (char* lpClassNamePtr = lpClassName)
    fixed (char* lpWindowNamePtr = lpWindowName)
    {
        return PInvoke.CreateWindowEx(dwExStyle, lpClassNamePtr, lpWindowNamePtr, dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam)
            .SuccessIsNotNull()
            .WithLastError();
    }
}
