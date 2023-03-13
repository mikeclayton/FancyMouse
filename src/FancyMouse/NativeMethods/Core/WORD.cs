namespace FancyMouse.NativeMethods.Core;

/// <summary>
/// A 16-bit unsigned integer.The range is 0 through 65535 decimal.
/// This type is declared in WinDef.h as follows:
/// typedef unsigned short WORD;
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct WORD
{
    public readonly ushort Value;

    public WORD(ushort value)
    {
        this.Value = value;
    }

    public static implicit operator ulong(WORD value) => value.Value;

    public static implicit operator WORD(ushort value) => new(value);
}
