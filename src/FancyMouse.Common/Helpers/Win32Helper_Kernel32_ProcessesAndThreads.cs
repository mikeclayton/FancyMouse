using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
    public static partial class Kernel32
    {
        public static DWORD GetCurrentThreadId()
        {
            return NativeMethods.Kernel32.GetCurrentThreadId();
        }
    }
}
