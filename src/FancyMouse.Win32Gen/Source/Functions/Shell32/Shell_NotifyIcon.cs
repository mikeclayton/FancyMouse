internal static Win32Result<BOOL> Shell_NotifyIcon(NOTIFY_ICON_MESSAGE dwMessage, in NOTIFYICONDATAW lpData)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw
    // Returns TRUE if successful, or FALSE otherwise.
    // If dwMessage is set to NIM_SETVERSION, the function returns
    //   TRUE if the version was successfully changed, or
    //   FALSE if the requested version is not supported.
    return PInvoke.Shell_NotifyIcon(dwMessage, lpData)
        .SuccessIsNonZero();
}
