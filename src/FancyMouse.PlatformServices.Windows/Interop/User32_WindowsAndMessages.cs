using static FancyMouse.PlatformServices.Windows.NativeMethods.User32;

namespace FancyMouse.PlatformServices.Windows.Interop;

internal static partial class User32
{
    internal static int GetSystemMetrics(SYSTEM_METRICS_INDEX smIndex)
    {
        var result = User32.GetSystemMetrics(smIndex);
        ResultHandler.ThrowIfZero(result, getLastError: false);
        return result;
    }
}
