internal static Win32Result<int> GetSystemMetrics(SYSTEM_METRICS_INDEX nIndex)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics
    // If the function succeeds, the return value is the requested system metric or configuration setting.
    // If the function fails, the return value is 0.
    // GetLastError does not provide extended error information, so it isn't
    // captured here even though 0 is treated as failure.
    return PInvoke.GetSystemMetrics(nIndex)
        .SuccessIsNonZero();
}
