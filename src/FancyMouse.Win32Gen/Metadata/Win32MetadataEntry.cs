namespace FancyMouse.Win32Gen.Metadata;

/// <summary>
/// One name indexed out of a win32metadata .winmd file by <see cref="Win32MetadataDirectory"/>.
/// </summary>
internal sealed record Win32MetadataEntry(string Name, Win32MemberKind Kind);
