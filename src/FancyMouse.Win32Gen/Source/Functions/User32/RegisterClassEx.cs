public static Win32Result<ushort> RegisterClassEx(in WNDCLASSEXW param0)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerclassexw
    // If the function succeeds, the return value is a class atom that uniquely identifies the class being registered.
    // If the function fails, the return value is zero.
    // To get extended error information, call GetLastError.
    return PInvoke.RegisterClassEx(param0)
        .SuccessIsNonZero()
        .WithLastError();
}
