internal static Win32Result<HRGN> CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy)
{
    // https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createroundrectrgn
    // If the function succeeds, the return value is the handle to the region.
    // If the function fails, the return value is NULL.
    return PInvoke.CreateRoundRectRgn(x1, y1, x2, y2, cx, cy)
        .SuccessIsNotNull();
}
