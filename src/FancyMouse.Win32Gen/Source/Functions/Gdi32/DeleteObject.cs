internal static Win32Result<BOOL> DeleteObject(HGDIOBJ ho)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-deleteobject
    // If the function succeeds, the return value is nonzero.
    // If the specified handle is not valid or is currently selected into a DC, the return value is zero.
    return PInvoke.DeleteObject(ho)
        .SuccessIsNonZero();
}
