using System.Runtime.InteropServices;

namespace FancyMouse.Win32.NativeMethods;

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
#pragma warning disable CA1720 // Identifier contains type name
    public readonly struct WCHAR
#pragma warning restore CA1720 // Identifier contains type name
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly char Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

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
