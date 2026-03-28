using System.Runtime.InteropServices;

namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// An unsigned INT_PTR.
    /// This type is declared in BaseTsd.h as follows:
    /// #if defined(_WIN64)
    ///  typedef unsigned __int64 UINT_PTR;
    /// #else
    ///  typedef unsigned int UINT_PTR;
    /// #endif
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct UINT_PTR
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly UIntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public UINT_PTR(UIntPtr value)
        {
            this.Value = value;
        }

        public static implicit operator UIntPtr(UINT_PTR value) => value.Value;

        public static explicit operator UINT_PTR(UIntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
