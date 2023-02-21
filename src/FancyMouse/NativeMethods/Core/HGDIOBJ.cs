namespace FancyMouse.NativeMethods.Core;

internal readonly struct HGDIOBJ
{

    public static readonly HGDIOBJ Null = new(IntPtr.Zero);

    public readonly IntPtr Value;

    public HGDIOBJ(IntPtr value)
    {
        this.Value = value;
    }

    public bool IsNull
    {
        get
        {
            return (this.Value == HGDIOBJ.Null.Value);
        }
    }

}
