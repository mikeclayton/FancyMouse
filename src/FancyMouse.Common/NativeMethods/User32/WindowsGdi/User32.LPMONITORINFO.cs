using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-monitorinfo
    /// </remarks>
    internal readonly struct LPMONITORINFO
    {
        public static readonly LPMONITORINFO Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public LPMONITORINFO(IntPtr value)
        {
            this.Value = value;
        }

        public LPMONITORINFO(MONITORINFO value)
        {
            this.Value = LPMONITORINFO.ToPtr(value);
        }

        public MONITORINFO ToStructure()
        {
            return Marshal.PtrToStructure<MONITORINFO>(this.Value);
        }

        private static IntPtr ToPtr(MONITORINFO value)
        {
            var ptr = Marshal.AllocHGlobal(MONITORINFO.Size);
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
