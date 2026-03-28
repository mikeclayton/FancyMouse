using System.Runtime.InteropServices;

namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    public readonly struct LPRECT
    {
        public static readonly LPRECT Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public LPRECT(IntPtr value)
        {
            this.Value = value;
        }

        public LPRECT(RECT value)
        {
            this.Value = LPRECT.ToPtr(value);
        }

        public bool IsNull => this.Value == LPRECT.Null.Value;

        private static IntPtr ToPtr(RECT value)
        {
            var ptr = Marshal.AllocHGlobal(RECT.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public static implicit operator IntPtr(LPRECT value) => value.Value;

        public static explicit operator LPRECT(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
