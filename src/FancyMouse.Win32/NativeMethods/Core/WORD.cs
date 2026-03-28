namespace FancyMouse.Win32.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A 16-bit unsigned integer.The range is 0 through 65535 decimal.
    /// This type is declared in WinDef.h as follows:
    /// typedef unsigned short WORD;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct WORD
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly ushort Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public WORD(ushort value)
        {
            this.Value = value;
        }

        public static implicit operator ulong(WORD value) => value.Value;

        public static implicit operator WORD(ushort value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
