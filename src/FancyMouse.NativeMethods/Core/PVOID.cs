using System.Runtime.InteropServices;

namespace FancyMouse.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// A pointer to any type.
    /// This type is declared in WinNT.h as follows:
    /// typedef void* PVOID;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct PVOID
    {
        public static readonly PVOID Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public PVOID(IntPtr value)
        {
            this.Value = value;
        }

        public static implicit operator IntPtr(PVOID value) => value.Value;

        public static implicit operator PVOID(IntPtr value) => new(value);

        public static PVOID Allocate(int length)
        {
            var ptr = Marshal.AllocHGlobal(length);
            return new PVOID(ptr);
        }

        public string? PtrToStringUni()
        {
            return Marshal.PtrToStringUni(this.Value);
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
