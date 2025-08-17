using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class User32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmessagew
    /// </remarks>
    public readonly struct LPMSG
    {
        public static readonly LPMSG Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public LPMSG(IntPtr value)
        {
            this.Value = value;
        }

        public LPMSG(MSG value)
        {
            this.Value = LPMSG.ToPtr(value);
        }

        public MSG ToStructure()
        {
            return Marshal.PtrToStructure<MSG>(this.Value);
        }

        private static IntPtr ToPtr(MSG value)
        {
            var ptr = Marshal.AllocHGlobal(MSG.Size);
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
