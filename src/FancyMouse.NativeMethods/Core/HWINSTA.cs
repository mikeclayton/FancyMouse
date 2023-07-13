namespace FancyMouse.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// A handle to a window station.
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE WINSTA;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct HWINSTA
    {
        public static readonly HWINSTA Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public HWINSTA(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HWINSTA.Null.Value;

        public static implicit operator IntPtr(HWINSTA value) => value.Value;

        public static explicit operator HWINSTA(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
