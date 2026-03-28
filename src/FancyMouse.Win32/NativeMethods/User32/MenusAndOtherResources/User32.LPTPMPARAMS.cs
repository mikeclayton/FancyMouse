using System.Runtime.InteropServices;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <summary>
    /// Contains extended parameters for the TrackPopupMenuEx function.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-tpmparams
    /// </remarks>
    public readonly struct LPTPMPARAMS
    {
        public static readonly LPTPMPARAMS Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

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
