public static Win32Result<DPI_AWARENESS_CONTEXT> GetThreadDpiAwarenessContext()
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getthreaddpiawarenesscontext
    // This method will return the latest DPI_AWARENESS_CONTEXT sent to SetThreadDpiAwarenessContext.
    // If SetThreadDpiAwarenessContext was never called for this thread, then the return value
    // will equal the default DPI_AWARENESS_CONTEXT for the process.
    return PInvoke.GetThreadDpiAwarenessContext()
        .AlwaysSucceeds();
}
