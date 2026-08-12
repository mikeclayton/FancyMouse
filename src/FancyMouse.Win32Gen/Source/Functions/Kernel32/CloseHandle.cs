internal static Win32Result<BOOL> CloseHandle(HANDLE hObject)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/handleapi/nf-handleapi-closehandle
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.CloseHandle(hObject)
        .SuccessIsNonZero()
        .WithLastError();
}
