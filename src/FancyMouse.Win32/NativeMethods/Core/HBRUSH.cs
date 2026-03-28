using static FancyMouse.Win32.NativeMethods.User32;

namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to a brush.
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE HBRUSH;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct HBRUSH
    {
        public static readonly HBRUSH Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HBRUSH(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HBRUSH.Null.Value;

        public static implicit operator IntPtr(HBRUSH value) => value.Value;

        public static explicit operator HBRUSH(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
