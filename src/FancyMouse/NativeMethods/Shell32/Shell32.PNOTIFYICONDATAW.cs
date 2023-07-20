using System.Runtime.InteropServices;

namespace FancyMouse.NativeMethods;

internal static partial class Shell32
{
    internal readonly struct PNOTIFYICONDATAW
    {
        public static readonly PNOTIFYICONDATAW Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public PNOTIFYICONDATAW(IntPtr value)
        {
            this.Value = value;
        }

        public PNOTIFYICONDATAW(NOTIFYICONDATAW value)
        {
            this.Value = PNOTIFYICONDATAW.ToPtr(value);
        }

        private static IntPtr ToPtr(NOTIFYICONDATAW value)
        {
            var ptr = Marshal.AllocHGlobal(NOTIFYICONDATAW.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public NOTIFYICONDATAW ToStructure()
        {
            return Marshal.PtrToStructure<NOTIFYICONDATAW>(this.Value);
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public static implicit operator IntPtr(PNOTIFYICONDATAW value) => value.Value;

        public static implicit operator PNOTIFYICONDATAW(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
