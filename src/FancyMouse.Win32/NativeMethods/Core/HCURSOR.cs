namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to a cursor.
    /// This type is declared in WinDef.h as follows:
    /// typedef HICON HCURSOR;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct HCURSOR
    {
        public static readonly HCURSOR Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HCURSOR(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HCURSOR.Null.Value;

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
