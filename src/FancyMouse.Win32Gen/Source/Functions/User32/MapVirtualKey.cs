internal static Win32Result<uint> MapVirtualKey(uint uCode, MAP_VIRTUAL_KEY_TYPE uMapType)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mapvirtualkeyw
    // The return value is either a scan code, a virtual-key code,
    // or a character value, depending on the value of uCode and uMapType.
    // If there is no translation, the return value is zero.
    return PInvoke.MapVirtualKey(uCode, uMapType)
        .AlwaysSucceeds();
}
