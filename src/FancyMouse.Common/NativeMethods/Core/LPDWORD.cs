using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// A message parameter.
    /// This type is declared in WinDef.h as follows:
    /// typedef LONG_PTR LPARAM;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct LPDWORD
    {
        public static readonly LPDWORD Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public LPDWORD(IntPtr value)
        {
            this.Value = value;
        }

        public LPDWORD(DWORD value)
        {
            this.Value = LPDWORD.ToPtr(value);
        }

        public bool IsNull => this.Value == LPDWORD.Null.Value;

        private static IntPtr ToPtr(DWORD value)
        {
            var ptr = Marshal.AllocHGlobal(DWORD.Size);
            Marshal.StructureToPtr(value, ptr, false);
            return ptr;
        }

        public void Free()
        {
            Marshal.FreeHGlobal(this.Value);
        }

        public static implicit operator IntPtr(LPDWORD value) => value.Value;

        public static explicit operator LPDWORD(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
