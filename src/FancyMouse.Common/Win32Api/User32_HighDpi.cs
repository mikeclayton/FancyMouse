using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.UI.HiDpi;

namespace FancyMouse.Common.Win32Api;

internal static partial class User32
{
    internal static Win32Result<DPI_AWARENESS> GetAwarenessFromDpiAwarenessContext(DPI_AWARENESS_CONTEXT value)
    {
        var returnValue = PInvoke.GetAwarenessFromDpiAwarenessContext(value);

        // If the provided value is null or invalid,
        // this method will return DPI_AWARENESS_INVALID
        return new Win32Result<DPI_AWARENESS>(
            nameof(PInvoke.GetAwarenessFromDpiAwarenessContext),
            returnValue,
            isSuccess: returnValue != DPI_AWARENESS.DPI_AWARENESS_INVALID,
            useLastError: false,
            Marshal.GetLastPInvokeError());
    }

    internal static Win32Result<DPI_AWARENESS_CONTEXT> GetThreadDpiAwarenessContext()
    {
        return PInvoke.GetThreadDpiAwarenessContext()
            .AlwaysSucceeds();
    }
}
