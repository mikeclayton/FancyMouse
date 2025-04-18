namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A 32-bit signed integer.The range is -2147483648 through 2147483647 decimal.
    /// This type is declared in WinNT.h as follows:
    /// typedef long LONG;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
#pragma warning disable CA1716 // Identifiers should not match keywords
#pragma warning disable CA1720 // Identifier contains type name
    public readonly struct LONG
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly int Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public LONG(int value)
        {
            this.Value = value;
        }

        public static implicit operator int(LONG value) => value.Value;

        public static implicit operator LONG(int value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
