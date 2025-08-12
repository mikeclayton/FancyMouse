using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FancyMouse.Common.UnitTests")]

// we don't want analyzer warnings about the NativeMethods namespace
// because we create a folder for each partial class which means the
// namespace doesn't match the folder path.
[assembly: SuppressMessage(
    "Style",
    "SA1200:CheckNamespace",
    Justification = "Using partial class name in path")]

/*
[assembly: SuppressMessage(
    "Style",
    "SA1200:CheckNamespace",
    Justification = "Using partial class name in path",
    Scope = "namespaceanddescendants",
    Target = "~N:FancyMouse.Common.NativeMethods")]
*/
