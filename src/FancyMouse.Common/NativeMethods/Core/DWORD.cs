using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A 32-bit unsigned integer. The range is 0 through 4294967295 decimal.
    /// This type is declared in IntSafe.h as follows:
    /// typedef unsigned long DWORD;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct DWORD
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly uint Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public DWORD(uint value)
        {
            this.Value = value;
        }

        public static int Size =>
            Marshal.SizeOf<DWORD>();

        public static implicit operator uint(DWORD value) => value.Value;

        public static implicit operator DWORD(uint value) => new(value);

        public static explicit operator int(DWORD value) => (int)value.Value;

        public static explicit operator DWORD(int value) => new((uint)value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
