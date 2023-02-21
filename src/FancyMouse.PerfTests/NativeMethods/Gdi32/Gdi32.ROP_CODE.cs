namespace FancyMouse.PerfTests.NativeMethods;

internal static partial class Gdi32
{

    public enum ROP_CODE : uint
    {

        // see https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-bitblt

        BLACKNESS = 0x00000042,
        CAPTUREBLT = 0x40000000,
        DSTINVERT = 0x00550009,
        MERGECOPY = 0x00C000CA,
        MERGEPAINT = 0x00BB0226,
        NOMIRRORBITMAP = 0x80000000,
        NOTSRCCOPY = 0x00330008,
        NOTSRCERASE = 0x001100A6,
        PATCOPY = 0x00F00021,
        PATINVERT = 0x005A0049,
        PATPAINT = 0x00FB0A09,
        SRCAND = 0x008800C6,
        SRCCOPY = 0x00CC0020,
        SRCERASE = 0x00440328,
        SRCINVERT = 0x00660046,
        SRCPAINT = 0x00EE0086,
        WHITENESS = 0x00FF0062

    }

}