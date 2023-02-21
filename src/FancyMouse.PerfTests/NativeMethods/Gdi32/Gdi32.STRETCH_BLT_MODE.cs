namespace FancyMouse.PerfTests.NativeMethods;

internal static partial class Gdi32
{

    public enum STRETCH_BLT_MODE : int
    {
        BLACKONWHITE = 1,
        COLORONCOLOR = 3,
        HALFTONE = 4,
        WHITEONBLACK = 2,
        STRETCH_ANDSCANS = STRETCH_BLT_MODE.BLACKONWHITE,
        STRETCH_DELETESCANS = STRETCH_BLT_MODE.COLORONCOLOR,
        STRETCH_HALFTONE = STRETCH_BLT_MODE.HALFTONE,
        STRETCH_ORSCANS = STRETCH_BLT_MODE.WHITEONBLACK
    }

}
