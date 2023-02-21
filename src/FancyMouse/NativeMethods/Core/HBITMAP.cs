namespace FancyMouse.NativeMethods.Core;

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
            return (this.Value == HBITMAP.Null.Value);
        }
    }

    public static implicit operator HGDIOBJ(HBITMAP h) => new(h.Value);
    public static explicit operator HBITMAP(HGDIOBJ h) => new(h.Value);

}
