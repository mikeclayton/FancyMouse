public static Win32Result<BOOL> StretchBlt(HDC hdcDest, int xDest, int yDest, int wDest, int hDest, HDC hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, ROP_CODE rop)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-stretchblt
    // If the function succeeds, the return value is nonzero.
    // If the function fails, the return value is zero.
    return PInvoke.StretchBlt(hdcDest, xDest, yDest, wDest, hDest, hdcSrc, xSrc, ySrc, wSrc, hSrc, rop)
        .SuccessIsNonZero();
}
