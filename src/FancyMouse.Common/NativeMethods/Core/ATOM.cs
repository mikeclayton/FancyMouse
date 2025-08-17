using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// An atom. For more information, see About Atom Tables.
    /// This type is declared in WinDef.h as follows:
    /// typedef WORD ATOM;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct ATOM
    {
        [MarshalAs(UnmanagedType.U2)]
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly ushort Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public ATOM(ushort value)
        {
            this.Value = value;
        }

        public static implicit operator ushort(ATOM value) => value.Value;

        public static explicit operator ATOM(ushort value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
