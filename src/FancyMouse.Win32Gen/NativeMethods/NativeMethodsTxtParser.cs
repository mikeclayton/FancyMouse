using Microsoft.CodeAnalysis;

namespace FancyMouse.Win32Gen.NativeMethods;

/// <summary>
/// Just the line-parsing half of CsWin32's own SourceGenerator.Execute - the
/// part that turns each NativeMethods.txt line into a structured entry (a
/// plain api name, a "Module.*" wildcard, or a "-Name" exclusion). None of
/// the type-resolution, banned-api checking, or diagnostics that follow it
/// in CsWin32 are reproduced here - we only need to know which entries are
/// being asked for, not to resolve or generate them ourselves.
/// </summary>
internal static class NativeMethodsTxtParser
{
    // CsWin32 also trims these zero-width characters (byte-order mark and
    // zero-width space, code points 0x FEFF and 0x 200B) off each name -
    // easy to end up with one pasted in by accident. Written as escape
    // sequences rather than the literal characters so they're visible.
    private static readonly char[] ZeroWidthWhitespace = new[]
    {
        (char)0xFEFF,
        (char)0x200B,
    };

    public static NativeMethodsEntries Parse(AdditionalText file, CancellationToken cancellationToken)
    {
        var entries = new List<NativeMethodsEntry>();

        var text = file.GetText(cancellationToken);
        if (text is null)
        {
            return new NativeMethodsEntries(entries);
        }

        foreach (var line in text.Lines)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var raw = line.ToString();
            if (string.IsNullOrWhiteSpace(raw) || raw.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            var location = Location.Create(file.Path, line.Span, text.Lines.GetLinePositionSpan(line.Span));

            if (raw.StartsWith("-", StringComparison.Ordinal))
            {
                var excluded = raw.Substring(1).Trim();
                if (excluded.Length > 0)
                {
                    entries.Add(new NativeMethodsEntry(NativeMethodsEntryKind.Exclusion, excluded, location));
                }

                continue;
            }

            var name = raw.Trim().Trim(NativeMethodsTxtParser.ZeroWidthWhitespace);
            if (name.Length == 0)
            {
                continue;
            }

            entries.Add(name.EndsWith(".*", StringComparison.Ordinal)
                ? new NativeMethodsEntry(NativeMethodsEntryKind.ModuleWildcard, name.Substring(0, name.Length - 2), location)
                : new NativeMethodsEntry(NativeMethodsEntryKind.ApiName, name, location));
        }

        return new NativeMethodsEntries(entries);
    }
}
