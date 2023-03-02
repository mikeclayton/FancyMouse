namespace FancyMouse.NativeMethods.Core;

/// <summary>
/// A handle to a device context (DC).
/// This type is declared in WinDef.h as follows:
/// typedef HANDLE HDC;
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
/// </remarks>
internal readonly struct HDC
{
    public static readonly HDC Null = new(IntPtr.Zero);

    public readonly IntPtr Value;

    public HDC(IntPtr value)
    {
        this.Value = value;
    }

    public bool IsNull => this.Value == HDC.Null.Value;
}
