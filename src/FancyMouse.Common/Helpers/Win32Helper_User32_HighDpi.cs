using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
    public static partial class User32
    {
        public static UINT GetDpiForWindow(HWND hwnd)
        {
            return NativeMethods.User32.GetDpiForWindow(hwnd);
        }
    }
}
