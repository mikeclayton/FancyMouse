namespace FancyMouse.NativeMethods.Core;

internal readonly struct HDC
{

    public static readonly HDC Null = new(IntPtr.Zero);

    public readonly IntPtr Value;

    public HDC(IntPtr value)
    {
        this.Value = value;
    }

    public bool IsNull
    {
        get
        {
            return (this.Value == HDC.Null.Value);
        }
    }

}
