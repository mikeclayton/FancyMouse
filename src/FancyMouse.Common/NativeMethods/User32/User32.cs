using System.Diagnostics.CodeAnalysis;

namespace FancyMouse.Common.NativeMethods;

[SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
internal static partial class User32
{
    public const uint CW_USEDEFAULT = 0x80000000;
}
