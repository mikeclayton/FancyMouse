public static Win32Result<BOOL> GetMonitorInfo(HMONITOR hMonitor, ref MONITORINFO lpmi)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmonitorinfow
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    return PInvoke.GetMonitorInfo(hMonitor, ref lpmi)
        .SuccessIsNonZero();
}
