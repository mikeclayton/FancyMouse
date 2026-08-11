public static Win32Result<uint> SendInput(ReadOnlySpan<INPUT> pInputs, int cbSize)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput
    // The function returns the number of events that it successfully inserted into the keyboard or mouse input stream.
    // If the function returns zero, the input was already blocked by another thread.
    // To get extended error information, call GetLastError.
    // This function fails when it is blocked by UIPI.
    // Note that neither GetLastError nor the return value will indicate the failure was caused by UIPI blocking.
    return PInvoke.SendInput(pInputs, cbSize)
        .SuccessIsNonZero()
        .WithLastError();
}
