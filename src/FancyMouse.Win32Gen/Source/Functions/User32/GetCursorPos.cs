internal static Win32Result<BOOL> GetCursorPos(out Point point)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getcursorpos
    // Returns nonzero if successful or zero otherwise.
    // To get extended error information, call GetLastError.
    return PInvoke.GetCursorPos(out point)
        .SuccessIsNonZero()
        .WithLastError();
}
