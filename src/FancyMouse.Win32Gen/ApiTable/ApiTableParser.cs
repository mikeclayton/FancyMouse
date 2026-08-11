using System.Collections.Immutable;

namespace FancyMouse.Win32Gen.ApiTable;

/// <summary>
/// Parses the <c>ApiTable.txt</c> format - one api per line, a run of
/// bracketed attribute tags followed by the api name, e.g.:
/// <code>
/// [SuccessIsNonZero] [UsesLastError] AppendMenu
/// </code>
/// Blank lines and lines starting with <c>//</c> are ignored, same
/// convention as <c>NativeMethods.txt</c>.
/// </summary>
internal static class ApiTableParser
{
    public static ApiTable Parse(string text)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, ApiEntry>(StringComparer.Ordinal);

        foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            var entry = ApiTableParser.ParseLine(line);
            builder[entry.ApiName] = entry;
        }

        return new ApiTable(builder.ToImmutable());
    }

    private static ApiEntry ParseLine(string line)
    {
        var attributes = ImmutableArray.CreateBuilder<ApiAttributeKind>();
        var remaining = line;

        while (true)
        {
            remaining = remaining.TrimStart();
            if (!remaining.StartsWith("[", StringComparison.Ordinal))
            {
                break;
            }

            var closeIndex = remaining.IndexOf(']');
            if (closeIndex < 0)
            {
                throw new FormatException($"Unterminated attribute in ApiTable.txt line: '{line}'");
            }

            // the placeholder [SuccessIsCustom] carries no argument yet -
            // once it does, this is where a "Tag(Argument)" shape would get
            // split apart before the kind lookup below.
            var tag = remaining.Substring(1, closeIndex - 1).Trim();
            attributes.Add(ApiTableParser.ParseAttributeKind(tag, line));
            remaining = remaining.Substring(closeIndex + 1);
        }

        var apiName = remaining.Trim();
        if (apiName.Length == 0)
        {
            throw new FormatException($"Missing api name in ApiTable.txt line: '{line}'");
        }

        return new ApiEntry(apiName, attributes.ToImmutable());
    }

    private static ApiAttributeKind ParseAttributeKind(string tag, string line)
        => tag switch
        {
            "SuccessIsNonZero" => ApiAttributeKind.SuccessIsNonZero,
            "SuccessIsNotNull" => ApiAttributeKind.SuccessIsNotNull,
            "AlwaysSucceeds" => ApiAttributeKind.AlwaysSucceeds,
            "SuccessIsCustom" => ApiAttributeKind.SuccessDelegate,
            "UsesLastError" => ApiAttributeKind.UseLastError,
            "HumanVerified" => ApiAttributeKind.HumanVerified,
            _ => throw new FormatException($"Unrecognized attribute '[{tag}]' in ApiTable.txt line: '{line}'"),
        };
}
