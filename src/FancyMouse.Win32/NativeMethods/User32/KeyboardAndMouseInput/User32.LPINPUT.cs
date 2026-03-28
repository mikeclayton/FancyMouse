using System.Runtime.InteropServices;

namespace FancyMouse.Win32.NativeMethods;

public static partial class User32
{
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput
    /// </remarks>
    public readonly struct LPINPUT
    {
        public static readonly LPINPUT Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public LPINPUT(IntPtr value)
        {
            this.Value = value;
        }

        public LPINPUT(INPUT[] values)
        {
            this.Value = LPINPUT.ToPtr(values);
        }

        public INPUT ToStructure()
        {
            return Marshal.PtrToStructure<INPUT>(this.Value);
        }

        public IEnumerable<INPUT> ToStructure(int count)
        {
            var ptr = this.Value;
            var size = INPUT.Size;
            for (var i = 0; i < count; i++)
            {
                yield return Marshal.PtrToStructure<INPUT>(this.Value);
                ptr += size;
            }
        }

        private static IntPtr ToPtr(INPUT[] values)
        {
            var mem = Marshal.AllocHGlobal(INPUT.Size * values.Length);
            var ptr = mem;
            var size = INPUT.Size;
            foreach (var value in values)
            {
                Marshal.StructureToPtr(value, ptr, false);
                ptr += size;
            }

            return mem;
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
