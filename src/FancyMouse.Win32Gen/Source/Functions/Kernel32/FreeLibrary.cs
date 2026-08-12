internal static Win32Result<BOOL> FreeLibrary(HMODULE hLibModule)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-freelibrary
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    // To get extended error information, call the GetLastError function.
    return PInvoke.FreeLibrary(hLibModule)
        .SuccessIsNonZero()
        .WithLastError();
}
