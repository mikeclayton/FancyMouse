namespace FancyMouse.NativeMethods.Core;

internal readonly struct HWND
{

    public static readonly HWND Null = new(IntPtr.Zero);

    public readonly IntPtr Value;

    public HWND(IntPtr value)
    {
        this.Value = value;
    }

    public bool IsNull
    {
        get
        {
            return (this.Value == HWND.Null.Value);
        }
    }

}
