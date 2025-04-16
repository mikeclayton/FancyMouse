namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// An unsigned INT. The range is 0 through 4294967295 decimal.
    /// This type is declared in WinDef.h as follows:
    /// typedef unsigned int UINT;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
#pragma warning disable CA1720 // Identifier contains type name
    public readonly struct UINT
#pragma warning restore CA1720 // Identifier contains type name
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly uint Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public UINT(uint value)
        {
            this.Value = value;
        }

        public static implicit operator uint(UINT value) => value.Value;

        public static implicit operator UINT(uint value) => new(value);

        public static explicit operator int(UINT value) => (int)value.Value;

        public static explicit operator UINT(int value) => new((uint)value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
