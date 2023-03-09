namespace FancyMouse.NativeMethods.Core;

/// <summary>
/// A pointer to any type.
/// This type is declared in WinNT.h as follows:
/// typedef void* PVOID;
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct PVOID
{
    public readonly IntPtr Value;

    public PVOID(IntPtr value)
    {
        this.Value = value;
    }

    public static implicit operator IntPtr(PVOID value) => value;

    public static implicit operator PVOID(IntPtr value) => new(value);
}
