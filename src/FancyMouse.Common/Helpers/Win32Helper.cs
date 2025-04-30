using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;
using static FancyMouse.Common.NativeMethods.User32;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
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
