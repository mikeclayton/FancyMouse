using static FancyMouse.PlatformServices.Windows.NativeMethods.Core;
using static FancyMouse.PlatformServices.Windows.NativeMethods.User32;

namespace FancyMouse.PlatformServices.Windows.Interop;

internal static partial class User32
{
    internal static UINT SendInput(UINT cInputs, LPINPUT pInputs, int cbSize)
    {
        var result = NativeMethods.User32.SendInput(cInputs, pInputs, cbSize);
        if (result != cInputs)
        {
            ResultHandler.HandleFailure((int)result, getLastError: true);
        }

        return result;
    }
}
