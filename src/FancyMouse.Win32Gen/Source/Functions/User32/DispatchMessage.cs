public static Win32Result<LRESULT> DispatchMessage(in MSG lpMsg)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-dispatchmessage
    // The return value specifies the value returned by the window procedure.
    // Although its meaning depends on the message being dispatched, the return value generally is ignored.
    return PInvoke.DispatchMessage(lpMsg)
        .AlwaysSucceeds();
}
