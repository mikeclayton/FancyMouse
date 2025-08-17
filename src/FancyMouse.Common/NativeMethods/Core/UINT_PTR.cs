using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

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
    internal readonly struct UINT_PTR
    {
        public readonly UIntPtr Value;

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
