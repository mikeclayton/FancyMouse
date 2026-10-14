internal static unsafe Win32Result<int> GetKeyNameText(int lParam, Span<char> lpString)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getkeynametextw
    // If the function succeeds, a null-terminated string is copied into the specified buffer,
    // and the return value is the length of the string, in characters, not counting the terminating null character.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.GetKeyNameText(lParam, lpString)
        .SuccessIsNonZero()
        .WithLastError();
}
