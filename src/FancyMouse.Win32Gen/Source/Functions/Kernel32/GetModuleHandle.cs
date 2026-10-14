internal static Win32Result<HMODULE> GetModuleHandle(PCWSTR lpModuleName)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-getmodulehandlew
    // If the function succeeds, the return value is a handle to the specified module.
    // If the function fails, the return value is NULL.
    // To get extended error information, call GetLastError.
    //
    // the returned handle doesn't increment the module's reference count, so it
    // must *not* be passed to FreeLibrary. this wraps the raw HMODULE overload
    // rather than CsWin32's string overload because that one returns a
    // FreeLibrarySafeHandle, which calls FreeLibrary when it's disposed.
    return PInvoke.GetModuleHandle(lpModuleName)
        .SuccessIsNotNull()
        .WithLastError();
}
