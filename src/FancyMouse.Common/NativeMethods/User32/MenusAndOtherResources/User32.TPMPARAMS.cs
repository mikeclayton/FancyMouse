using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Contains extended parameters for the TrackPopupMenuEx function.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-tpmparams
    /// </remarks>
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Names match Win32 api")]
    internal readonly struct TPMPARAMS
    {
        public readonly UINT cbSize;
        public readonly RECT rcExclude;

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
