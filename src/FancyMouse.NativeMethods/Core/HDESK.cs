namespace FancyMouse.NativeMethods;

internal static partial class Core
{
    /// <summary>
    /// A handle to a desktop.
    /// This type is declared in WinDef.h as follows:
    /// typedef HANDLE HDESK;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct HDESK
    {
        public static readonly HDESK Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public HDESK(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HDESK.Null.Value;

        public static implicit operator IntPtr(HDESK value) => value.Value;

        public static implicit operator HDESK(IntPtr value) => new(value);

        public static implicit operator HANDLE(HDESK value) => new(value.Value);

        public static implicit operator HDESK(HANDLE value) => new(value.Value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
