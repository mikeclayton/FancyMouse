namespace FancyMouse.NativeMethods.Core;

/// <summary>
/// A handle to an object.
/// This type is declared in WinNT.h as follows:
/// typedef PVOID HANDLE;
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct HANDLE
{
    public static readonly HANDLE Null = new(IntPtr.Zero);

    public readonly IntPtr Value;

    public HANDLE(IntPtr value)
    {
        this.Value = value;
    }

    public bool IsNull
    {
        get
        {
            return this.Value == HANDLE.Null.Value;
        }
    }

    public static implicit operator uint(HANDLE value) => (uint)value.Value;

    public static implicit operator HANDLE(uint value) => new(new IntPtr(value));
}
