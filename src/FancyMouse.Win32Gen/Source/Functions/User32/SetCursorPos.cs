internal static Win32Result<BOOL> SetCursorPos(int X, int Y)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcursorpos
    // Returns nonzero if successful or zero otherwise.
    // To get extended error information, call GetLastError.
    var result = PInvoke.SetCursorPos(X, Y)
        .SuccessIsNonZero()
        .WithLastError()
        .AsWin32Result();

    // SetCursorPos has been known to return zero (i.e. an apparent failure)
    // while GetLastError also returns zero - Win32 defines a zero last-error as
    // meaning the call actually succeeded, so that specific combination is
    // treated as success too.
    if (result.Failure && result.LastError == 0)
    {
        result = result.WithSuccess();
    }

    return result;
}
