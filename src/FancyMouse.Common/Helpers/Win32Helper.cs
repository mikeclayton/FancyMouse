using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
    internal static Win32Exception NewWin32Exception(UINT hResult, [CallerMemberName] string memberName = "")
    {
        var message = string.Join(
            Environment.NewLine,
            $"{memberName} failed with result {hResult}.");
        return new Win32Exception(message);
    }

    internal static Win32Exception NewWin32Exception(UINT hResult, int lastError, [CallerMemberName] string memberName = "")
    {
        var message = string.Join(
            Environment.NewLine,
            $"{memberName} failed with result {hResult}.",
            $"{nameof(Marshal.GetLastPInvokeError)} returned '{lastError}'",
            new Win32Exception(lastError).Message);
        return new Win32Exception(lastError, message);
    }
}
