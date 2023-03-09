namespace FancyMouse.NativeMethods.Core;

/// <summary>
/// A 32-bit signed integer.The range is -2147483648 through 2147483647 decimal.
/// This type is declared in WinNT.h as follows:
/// typedef long LONG;
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct LONG
{
    public readonly long Value;

    public LONG(long value)
    {
        this.Value = value;
    }

    public static implicit operator long(LONG value) => value;

    public static implicit operator LONG(long value) => new(value);
}
