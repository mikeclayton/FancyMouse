namespace FancyMouse.Common.NativeMethods;

public static partial class Core
{
    /// <summary>
    /// A message parameter.
    /// This type is declared in WinDef.h as follows:
    /// typedef UINT_PTR WPARAM;
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/windows/win32/winprog/windows-data-types
    /// </remarks>
    public readonly struct WPARAM
    {
        public static readonly WPARAM Null = new(UIntPtr.Zero);

#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly UIntPtr Value;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public WPARAM(UIntPtr value)
        {
            this.Value = value;
        }

        public static implicit operator UIntPtr(WPARAM value) => value.Value;

        public static explicit operator WPARAM(UIntPtr value) => new(value);

        public override string ToString()
        {
            return $"{this.GetType().Name}({this.Value})";
        }
    }
}
