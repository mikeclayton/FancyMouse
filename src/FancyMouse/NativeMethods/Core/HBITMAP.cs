namespace FancyMouse.NativeMethods.Core;

/// <summary>
/// A handle to a bitmap.
/// This type is declared in WinDef.h as follows:
/// typedef HANDLE HBITMAP;
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct HBITMAP
{
    public static readonly HBITMAP Null = new(IntPtr.Zero);

    public readonly IntPtr Value;

    public HBITMAP(IntPtr value)
    {
        this.Value = value;
    }

    public bool IsNull
    {
        get
        {
            return this.Value == HBITMAP.Null.Value;
        }
    }

    public static implicit operator HGDIOBJ(HBITMAP h) => new(h.Value);

    public static explicit operator HBITMAP(HGDIOBJ h) => new(h.Value);
}
