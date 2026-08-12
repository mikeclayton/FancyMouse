internal static Win32Result<int> GetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongw
    // If the function succeeds, the return value is the requested value.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    // If SetWindowLong has not been called previously, GetWindowLong returns zero for values in the extra window or class memory.

    // a zero result is ambiguous - it can mean either "the value has never
    // been set" or "the call failed" - the documented way to tell them apart
    // is to clear the last-error value immediately before the call, then only
    // treat a zero result as a failure if the last error is also non-zero.
    Marshal.SetLastPInvokeError(0);
    var result = PInvoke.GetWindowLong(hWnd, nIndex)
        .SuccessIsNonZero()
        .WithLastError()
        .AsWin32Result();

    if (result.Failure && result.LastError == 0)
    {
        result = result.WithSuccess();
    }

    return result;
}
