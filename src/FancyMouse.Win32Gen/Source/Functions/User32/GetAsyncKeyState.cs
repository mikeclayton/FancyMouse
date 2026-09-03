internal static Win32Result<short> GetAsyncKeyState(int vKey)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getasynckeystate
    // The return value is zero if the call fails
    return PInvoke.GetAsyncKeyState(vKey)
        .SuccessIsNonZero();
}
