internal static Win32Result<LRESULT> CallNextHookEx(HHOOK hhk, int nCode, WPARAM wParam, LPARAM lParam)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-callnexthookex
    // This value is returned by the next hook procedure in the chain.
    // The current hook procedure must also return this value.
    // The meaning of the return value depends on the hook type.
    return PInvoke.CallNextHookEx(hhk, nCode, wParam, lParam)
        .AlwaysSucceeds();
}
