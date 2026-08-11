public static Win32Result<HMONITOR> MonitorFromPoint(Point pt, MONITOR_FROM_FLAGS dwFlags)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-monitorfrompoint
    // If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor.
    // If the point is not contained by a display monitor, the return value depends on the value of dwFlags.
    return PInvoke.MonitorFromPoint(pt, dwFlags)
        .SuccessIsNotNull();
}
