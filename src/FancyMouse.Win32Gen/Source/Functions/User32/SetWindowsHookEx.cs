internal static Win32Result<HHOOK> SetWindowsHookEx(WINDOWS_HOOK_ID idHook, HOOKPROC lpfn, HINSTANCE hmod, uint dwThreadId)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexw
    // If the function succeeds, the return value is the handle to the hook procedure.
    // If the function fails, the return value is NULL.
    // To get extended error information, call GetLastError.
    return PInvoke.SetWindowsHookEx(idHook, lpfn, hmod, dwThreadId)
        .SuccessIsNotNull()
        .WithLastError();
}
