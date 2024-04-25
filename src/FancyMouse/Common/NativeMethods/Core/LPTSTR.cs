using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// An LPWSTR if UNICODE is defined, an LPSTR otherwise. For more information, see Windows Data Types for Strings.
    /// This type is declared in WinNT.h as follows:
    /// C++
    /// #ifdef UNICODE
    ///  typedef LPWSTR LPTSTR;
    /// #else
    ///  typedef LPSTR LPTSTR;
    /// #endif
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct LPTSTR
    {
        public static readonly LPTSTR Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public LPTSTR(IntPtr value)
        {
            this.Value = value;
        }

        public LPTSTR(string value)
        {
            this.Value = Marshal.StringToHGlobalAuto(value);
        }

        public bool IsNull => this.Value == LPTSTR.Null.Value;

        public static implicit operator IntPtr(LPTSTR value) => value.Value;

        public static explicit operator LPTSTR(IntPtr value) => new(value);

        public static implicit operator string?(LPTSTR value) => Marshal.PtrToStringUni(value.Value);

        public static implicit operator LPTSTR(string value) => new(value);

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
