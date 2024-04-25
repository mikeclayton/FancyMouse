namespace FancyMouse.Common.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// A handle to a bitmap.
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE HBITMAP;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct HBITMAP
    {
        public static readonly HBITMAP Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public HBITMAP(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HBITMAP.Null.Value;

        public static implicit operator IntPtr(HBITMAP value) => value.Value;

        public static explicit operator HBITMAP(IntPtr value) => new(value);

        public static explicit operator HBITMAP(HGDIOBJ value) => new(value.Value);

        public static implicit operator HGDIOBJ(HBITMAP value) => new(value.Value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
