using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Contains extended parameters for the TrackPopupMenuEx function.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-tpmparams
    /// </remarks>
    internal readonly struct LPTPMPARAMS
    {
        public static readonly LPTPMPARAMS Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public LPTPMPARAMS(IntPtr value)
        {
            this.Value = value;
        }

        public LPTPMPARAMS(TPMPARAMS value)
        {
            this.Value = LPTPMPARAMS.ToPtr(value);
        }

        public TPMPARAMS ToStructure()
        {
            return Marshal.PtrToStructure<TPMPARAMS>(this.Value);
        }

        private static IntPtr ToPtr(TPMPARAMS value)
        {
            var ptr = Marshal.AllocHGlobal(TPMPARAMS.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
