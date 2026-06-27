using FancyMouse.Common.Win32Api;

using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace FancyMouse.Common.Win32Api;

internal static partial class User32
{
    internal static Win32Result<uint> SendInput(ReadOnlySpan<INPUT> pInputs, int cbSize)
    {
        // If the function returns zero, the input was already blocked by another thread.
        // To get extended error information, call GetLastError.
        return PInvoke.SendInput(pInputs, cbSize)
            .SuccessIsNonZero()
            .UsesLastError();
    }
}
