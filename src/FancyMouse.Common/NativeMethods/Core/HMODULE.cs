namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A handle to a module.This is the base address of the module in memory.
    /// HMODULE and HINSTANCE are the same in current versions of Windows, but represented different things in 16-bit Windows.
    /// This type is declared in WinDef.h as follows:
    /// typedef HINSTANCE HMODULE;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    internal readonly struct HMODULE
    {
        public static readonly HMODULE Null = new(IntPtr.Zero);

        public readonly IntPtr Value;

        public HMODULE(IntPtr value)
        {
            this.Value = value;
        }

        public bool IsNull => this.Value == HMODULE.Null.Value;

        public static implicit operator IntPtr(HMODULE value) => value.Value;

        public static explicit operator HMODULE(IntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
