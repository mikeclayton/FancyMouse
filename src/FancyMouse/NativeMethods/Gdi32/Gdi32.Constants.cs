using System.Diagnostics.CodeAnalysis;
using FancyMouse.NativeMethods.Core;

namespace FancyMouse.NativeMethods;

[SuppressMessage("SA1310", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Names match Win32 api")]
internal static partial class Gdi32
{
    public const uint GDI_ERROR = 0xFFFFFFFF;
    public static readonly HANDLE HGDI_ERROR = (HANDLE)GDI_ERROR;
}
