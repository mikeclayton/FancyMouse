internal static Win32Result<DPI_AWARENESS> GetAwarenessFromDpiAwarenessContext(DPI_AWARENESS_CONTEXT value)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getawarenessfromdpiawarenesscontext
    // If the provided value is null or invalid, this method will return DPI_AWARENESS_INVALID.
    var result = PInvoke.GetAwarenessFromDpiAwarenessContext(value);
    var success = result != DPI_AWARENESS.DPI_AWARENESS_INVALID;
    return new Win32Result<DPI_AWARENESS>(result, success);
}
