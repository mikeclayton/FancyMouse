using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.Interop;

public static partial class Kernel32
{
    public static DWORD GetCurrentThreadId()
    {
        return NativeMethods.Kernel32.GetCurrentThreadId();
    }
}
