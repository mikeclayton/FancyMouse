using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.Interop;

internal static class Win32Helper
{
    public static Win32Exception NewWin32Exception(UINT hResult, [CallerMemberName] string memberName = "")
    {
        var message = string.Join(
            Environment.NewLine,
            $"{memberName} failed with result {hResult}.");
        return new Win32Exception(message);
    }

    public static Win32Exception NewWin32Exception(UINT hResult, int lastError, [CallerMemberName] string memberName = "")
    {
        var message = string.Join(
            Environment.NewLine,
            $"{memberName} failed with result {hResult}.",
            $"{nameof(Marshal.GetLastPInvokeError)} returned '{lastError}'",
            new Win32Exception(lastError).Message);
        return new Win32Exception(lastError, message);
    }
}
