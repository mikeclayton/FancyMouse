public static Win32Result<uint> GetCurrentThreadId()
{
    // https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getcurrentthreadid
    // The return value is the thread identifier of the calling thread.
    return PInvoke.GetCurrentThreadId()
        .AlwaysSucceeds();
}
