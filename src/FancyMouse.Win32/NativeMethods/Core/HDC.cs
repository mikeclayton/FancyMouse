namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to a device context (DC).
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE HDC;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct HDC
    {
        public static readonly HDC Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HDC(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HDC.Null.Value;

        public static implicit operator IntPtr(HDC value) => value.Value;

        public static explicit operator HDC(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
