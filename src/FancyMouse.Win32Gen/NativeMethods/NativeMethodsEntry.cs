using Microsoft.CodeAnalysis;

namespace FancyMouse.Win32Gen.NativeMethods;

/// <summary>
/// One parsed, non-blank, non-comment line of a NativeMethods.txt file.
/// </summary>
internal sealed record NativeMethodsEntry(NativeMethodsEntryKind Kind, string Name, Location Location);
