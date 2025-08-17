namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to a GDI object.
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE HGDIOBJ;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct HGDIOBJ
    {
        public static readonly HGDIOBJ Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public HGDIOBJ(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HGDIOBJ.Null.Value;

        public static implicit operator IntPtr(HGDIOBJ value) => value.Value;

        public static explicit operator HGDIOBJ(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
