using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to a window.
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE HWND;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct HWND
    {
        public static readonly HWND Null = new(IntPtr.Zero);

        [SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
        public static readonly HWND HWND_MESSAGE = new(-3);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HWND(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HWND.Null.Value;

        public static implicit operator IntPtr(HWND value) => value.Value;

        public static explicit operator HWND(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
