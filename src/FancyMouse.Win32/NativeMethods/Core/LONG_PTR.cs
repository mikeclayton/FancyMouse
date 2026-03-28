using System.Reflection;
using System.Runtime.InteropServices;

namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A signed long type for pointer precision.
    /// Use when casting a pointer to a long to perform pointer arithmetic.
    /// This type is declared in BaseTsd.h as follows:
    /// C++
    /// #if defined(_WIN64)
    ///  typedef __int64 LONG_PTR;
    /// #else
    ///  typedef long LONG_PTR;
    /// #endif
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct LONG_PTR
    {
        public static readonly LONG_PTR Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public LONG_PTR(IntPtr value)
        {
            this.Value = value;
        }

        public LONG_PTR(int size)
        {
            this.Value = Marshal.AllocHGlobal(size);
        }

        public bool IsNull => this.Value == LONG_PTR.Null.Value;

        public static implicit operator IntPtr(LONG_PTR value) => value.Value;

        public static explicit operator LONG_PTR(IntPtr value) => new(value);

        public static implicit operator string?(LONG_PTR value) => Marshal.PtrToStringUni(value.Value);

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
