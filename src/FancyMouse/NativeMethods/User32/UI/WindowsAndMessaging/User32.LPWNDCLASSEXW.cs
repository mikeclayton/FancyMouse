using System.Runtime.InteropServices;

namespace FancyMouse.NativeMethods;

internal static partial class User32
{
    internal readonly struct LPWNDCLASSEXW
    {
        public static readonly LPWNDCLASSEXW Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public LPWNDCLASSEXW(IntPtr value)
        {
            this.Value = value;
        }

        public LPWNDCLASSEXW(WNDCLASSEXW value)
        {
            this.Value = LPWNDCLASSEXW.ToPtr(value);
        }

        public bool IsNull => this.Value == LPWNDCLASSEXW.Null.Value;

        private static IntPtr ToPtr(WNDCLASSEXW value)
        {
            var ptr = Marshal.AllocHGlobal(WNDCLASSEXW.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public static implicit operator IntPtr(LPWNDCLASSEXW value) => value.Value;

        public static explicit operator LPWNDCLASSEXW(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
