using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.NativeMethods;

public static partial class Kernel32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/aa379560(v=vs.85)
    /// </remarks>
    [SuppressMessage("SA1304", "SA1304:NonPrivateReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "Names match Win32 api")]
    [SuppressMessage("SA1307", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Names match Win32 api")]
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct SECURITY_ATTRIBUTES
    {
        internal readonly DWORD nLength;
        internal readonly LPVOID lpSecurityDescriptor;
        internal readonly BOOL bInheritHandle;

        internal SECURITY_ATTRIBUTES(
            DWORD nLength, LPVOID lpSecurityDescriptor, BOOL bInheritHandle)
        {
            this.nLength = nLength;
            this.lpSecurityDescriptor = lpSecurityDescriptor;
            this.bInheritHandle = bInheritHandle;
        }

        public static int Size =>
            Marshal.SizeOf<SECURITY_ATTRIBUTES>();
    }
}
