using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.Win32.NativeMethods.Core;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Contains extended parameters for the TrackPopupMenuEx function.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-tpmparams
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Names match Win32 api")]
    public readonly struct TPMPARAMS
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly UINT cbSize;
        public readonly RECT rcExclude;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public TPMPARAMS(
            UINT cbSize,
            RECT rcExclude)
        {
            this.cbSize = cbSize;
            this.rcExclude = rcExclude;
        }

        public static int Size =>
            Marshal.SizeOf<TPMPARAMS>();
    }
}
