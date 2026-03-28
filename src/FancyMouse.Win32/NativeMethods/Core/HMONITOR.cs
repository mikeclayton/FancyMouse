namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to a display monitor.
    /// This type is declared in WinDef.h as follows:
    /// if(WINVER >= 0x0500) typedef HANDLE HMONITOR;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct HMONITOR
    {
        public static readonly HMONITOR Null = new(IntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly IntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public HMONITOR(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HMONITOR.Null.Value;

        public static implicit operator int(HMONITOR value) => value.Value.ToInt32();

        public static explicit operator HMONITOR(int value) => new(value);

        public static implicit operator IntPtr(HMONITOR value) => value.Value;

        public static explicit operator HMONITOR(IntPtr value) => new(value);

        public static implicit operator HANDLE(HMONITOR value) => new(value.Value);

        public static explicit operator HMONITOR(HANDLE value) => new(value.Value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
