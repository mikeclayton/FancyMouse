namespace FancyMouse.NativeMethods.Core;

/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct DWORD
{
    public readonly uint Value;

    public DWORD(uint value)
    {
        this.Value = value;
    }

    public static implicit operator uint(DWORD value) => value;

    public static implicit operator DWORD(uint value) => new(value);
}
