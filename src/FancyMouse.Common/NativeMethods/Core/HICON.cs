namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to an icon.
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE HICON;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct HICON
    {
        public static readonly HICON Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HICON(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HICON.Null.Value;

        public static implicit operator IntPtr(HICON value) => value.Value;

        public static explicit operator HICON(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
