namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A 16-bit Unicode character.For more information, see Character Sets Used By Fonts.
    /// This type is declared in WinNT.h as follows:
    /// typedef wchar_t WCHAR;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct WCHAR
    {
        public readonly char Value;

        public WCHAR(char value)
        {
            this.Value = value;
        }

        public static implicit operator char(WCHAR value) => value.Value;

        public static implicit operator WCHAR(char value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
