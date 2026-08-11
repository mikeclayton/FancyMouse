public static Win32Result<BOOL> TranslateMessage(in MSG lpMsg)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-translatemessage
    // If the message is translated (that is, a character message is posted to the thread's message queue),
    // the return value is nonzero.
    // If the message is WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP,
    // the return value is nonzero, regardless of the translation.
    // If the message is not translated(that is, a character message is not posted to the thread's message queue),
    // the return value is zero.

    // zero just means "this particular message wasn't translatable" - the
    // normal case for most messages in a loop, not a failure - so there's
    // no reliable failure signal here at all.
    return PInvoke.TranslateMessage(lpMsg)
        .AlwaysSucceeds();
}
