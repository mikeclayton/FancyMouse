namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to a menu.
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE HMENU;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct HMENU
    {
        public static readonly HMENU Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HMENU(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HMENU.Null.Value;

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
