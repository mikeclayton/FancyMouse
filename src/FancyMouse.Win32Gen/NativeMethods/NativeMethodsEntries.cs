namespace FancyMouse.Win32Gen.NativeMethods;

/// <summary>
/// A parsed collection of <see cref="NativeMethodsEntry"/> instances - e.g.
/// what <see cref="NativeMethodsTxtParser"/> produces from a
/// NativeMethods.txt file today - held together as a single named unit so
/// callers can consume already-parsed data (e.g.
/// <see cref="CsWin32.CsWin32Helper"/>) without touching the file
/// system themselves. Named for the entries rather than the ".txt" source
/// format, since a NativeMethods.json source is equally plausible later.
/// </summary>
internal sealed class NativeMethodsEntries
{
    public NativeMethodsEntries(IReadOnlyList<NativeMethodsEntry> entries)
    {
        this.Entries = entries;
    }

    public IReadOnlyList<NativeMethodsEntry> Entries { get; }
}
