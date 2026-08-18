internal static Win32Result<HRGN> CreateRectRgn(int x1, int y1, int x2, int y2)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createrectrgn
    // If the function succeeds, the return value is the handle to the region.
    // If the function fails, the return value is NULL.
    //
    // Also documented there: setting both diametrically-opposite corners to (0,0) is the
    // standard way to create an *empty* region - unlike CreateRoundRectRgn, which makes no
    // such guarantee for a degenerate (zero-size) rectangle.
    return PInvoke.CreateRectRgn(x1, y1, x2, y2)
        .SuccessIsNotNull();
}
