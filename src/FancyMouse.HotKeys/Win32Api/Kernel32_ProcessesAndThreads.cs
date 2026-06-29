using Windows.Win32;

namespace FancyMouse.HotKeys.Win32Api;

internal static partial class Kernel32
{
    internal static Win32Result<uint> GetCurrentThreadId()
    {
        // The return value is the thread identifier of the calling thread.
        return PInvoke.GetCurrentThreadId()
            .AlwaysSucceeds();
    }
}
