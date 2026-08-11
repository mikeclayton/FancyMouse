public static Win32Result<int> SetStretchBltMode(HDC hdc, STRETCH_BLT_MODE mode)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-setstretchbltmode
    // If the function succeeds, the return value is the previous stretching mode.
    // If the function fails, the return value is zero.
    // This function can return the following value.
    //   * ERROR_INVALID_PARAMETER - One or more of the input parameters is invalid.
    return PInvoke.SetStretchBltMode(hdc, mode)
        .SuccessIsNonZero();
}
