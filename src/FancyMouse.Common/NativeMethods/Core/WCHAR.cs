using System.Runtime.InteropServices;

namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A 16-bit Unicode character. For more information, see Character Sets Used By Fonts.
    /// This type is declared in WinNT.h as follows:
    /// typedef wchar_t WCHAR;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal readonly struct WCHAR
    {
        public readonly char Value;

        public WCHAR(char value)
        {
            this.Value = value;
        }

        public static implicit operator char(WCHAR value) => value.Value;

        public static implicit operator WCHAR(char value) => new(value);

        public static WCHAR[] AsNullTerminatedArray(string value, int length = -1)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (length < -1 || length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "length must be -1 or greater than 0.");
            }

            var resultLength = (length == -1) ? value.Length + 1 : length;
            var result = new WCHAR[resultLength];

            // copy characters from the string value into the array,
            // but make sure we leave space at the end for the null
            // terminator if the "length" parameter is specified
            var valueCharCount = Math.Min(value.Length, resultLength - 1);
            for (var i = 0; i < valueCharCount; i++)
            {
                result[i] = (WCHAR)value[i];
            }

            // fill the rest of the array with null characters
            // (there's always at least one index reserved for a null terminator)
            for (var i = valueCharCount; i < resultLength; i++)
            {
                result[i] = (WCHAR)0;
            }

            return result;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
