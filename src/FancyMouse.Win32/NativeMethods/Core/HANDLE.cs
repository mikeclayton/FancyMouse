namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to an object.
    /// This type is declared in WinNT.h as follows:
    /// typedef PVOID HANDLE;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct HANDLE
    {
        public static readonly HANDLE Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HANDLE(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HANDLE.Null.Value;

        public static implicit operator IntPtr(HANDLE value) => value.Value;

        public static explicit operator HANDLE(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
