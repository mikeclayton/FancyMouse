internal static Win32Result<int> SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, int dwNewLong)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongw
    // If the function succeeds, the return value is the previous value of the specified 32-bit integer.
    // If the function fails, the return value is zero. To get extended error information, call GetLastError.
    // If the previous value of the specified 32-bit integer is zero, and the function succeeds,
    // the return value is zero, but the function does not clear the last error information.
    // This makes it difficult to determine success or failure.
    // To deal with this, you should clear the last error information by calling SetLastError with 0
    // before calling SetWindowLong. Then, function failure will be indicated by a return value of
    // zero and a GetLastError result that is nonzero.

    // same ambiguous-zero shape as GetWindowLong (this is its write
    // counterpart) - clear the last-error immediately before the call, then
    // only treat a zero result as a failure if the last error is also
    // non-zero.
    Marshal.SetLastPInvokeError(0);
    var result = PInvoke.SetWindowLong(hWnd, nIndex, dwNewLong)
        .SuccessIsNonZero()
        .WithLastError()
        .AsWin32Result();

    if (result.Failure && result.LastError == 0)
    {
        result = result.WithSuccess();
    }

    return result;
}
