namespace FancyMouse.NativeMethods.Core;

/// <summary>
/// An unsigned LONG. The range is 0 through 4294967295 decimal.
/// This type is declared in WinDef.h as follows:
/// typedef unsigned long ULONG;
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct ULONG
{
    public readonly ulong Value;

    public ULONG(ulong value)
    {
        this.Value = value;
    }

    public static implicit operator ulong(ULONG value) => value;

    public static implicit operator ULONG(ulong value) => new(value);
}
