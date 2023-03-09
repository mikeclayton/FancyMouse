namespace FancyMouse.NativeMethods.Core;

/// <summary>
/// A byte (8 bits).
/// This type is declared in WinDef.h as follows:
/// typedef unsigned char BYTE;
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct BYTE
{
    public readonly byte Value;

    public BYTE(byte value)
    {
        this.Value = value;
    }

    public static implicit operator byte(BYTE value) => value;

    public static implicit operator BYTE(byte value) => new(value);
}
