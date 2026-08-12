internal static Win32Result<BOOL> GetMessage(out MSG lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessage
    // If the function retrieves a message other than WM_QUIT, the return value is nonzero.
    // If the function retrieves the WM_QUIT message, the return value is zero.
    // If there is an error, the return value is -1. To get extended error information, call GetLastError.

    // unlike the SuccessIsNonZero() functions, a zero result here (WM_QUIT)
    // is an expected, legitimate outcome the caller needs to see via
    // Value - not a failure - so only the documented -1 sentinel counts
    // as failure here.
    var result = PInvoke.GetMessage(out lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
    return (int)result == -1
        ? new(result, success: false, Marshal.GetLastPInvokeError())
        : new(result, success: true);
}
