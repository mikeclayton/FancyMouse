namespace FancyMouse.NativeMethods.Core;

/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct HBRUSH
{
    public static readonly HBRUSH Null = new(IntPtr.Zero);

    public readonly IntPtr Value;

    public HBRUSH(IntPtr value)
    {
        this.Value = value;
    }

    public bool IsNull => this.Value == HBRUSH.Null.Value;

    public static implicit operator IntPtr(HBRUSH value) => value.Value;

    public static implicit operator HBRUSH(IntPtr value) => new(value);
}
