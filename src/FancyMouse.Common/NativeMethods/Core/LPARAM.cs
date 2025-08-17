namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A message parameter.
    /// This type is declared in WinDef.h as follows:
    /// typedef LONG_PTR LPARAM;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct LPARAM
    {
        public static readonly LPARAM Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public LPARAM(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == LPARAM.Null.Value;

        public static implicit operator IntPtr(LPARAM value) => value.Value;

        public static explicit operator LPARAM(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
