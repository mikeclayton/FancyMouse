namespace FancyMouse.Win32Gen.ApiDocs;

/// <summary>
/// Renders a parsed <see cref="ApiDetails"/> entry as a ready-to-emit XML
/// doc comment block. Split out of <see cref="ApiDocsHelper"/> (which pairs
/// this with loading/caching/W-A-suffix-fallback concerns) purely so it can
/// be tested directly against hand-built <see cref="ApiDetails"/> instances,
/// without needing a real win32docs data file.
/// </summary>
/// <remarks>
/// Mirrors the exact shape CsWin32's own generated doc comments use for the
/// same win32docs data (verified against its real output for
/// GetCursorPos): <summary> stays a single inline line with no <para>;
/// <param>/<remarks> wrap their content in one <para>, plus a second
/// "Read more on learn.microsoft.com" <para> linking back to the api's
/// help page (with a "#parameters"/"#" suffix respectively); <returns>
/// gets just the one content <para>, no "Read more" companion. Tags
/// embedded in the raw text (<b>, <a href="...">, ...) are left
/// unescaped and passed straight through, same as CsWin32 does - the
/// win32docs data is trusted to already be well-formed.
/// </remarks>
internal static class ApiDocXmlCommentBuilder
{
    public static string Build(ApiDetails details)
    {
        var lines = new List<string>();

        if (!string.IsNullOrEmpty(details.Description))
        {
            lines.Add($"/// <summary>{ApiDocXmlCommentBuilder.CollapseToOneLine(details.Description!)}</summary>");
        }

        foreach (var parameter in details.Parameters)
        {
            lines.Add($"/// <param name=\"{parameter.Key}\">");
            lines.Add($"/// <para>{ApiDocXmlCommentBuilder.CollapseToOneLine(parameter.Value)}</para>");
            if (details.HelpLink is not null)
            {
                lines.Add($"/// <para><see href=\"{details.HelpLink}#parameters\">Read more on learn.microsoft.com</see>.</para>");
            }

            lines.Add("/// </param>");
        }

        if (!string.IsNullOrEmpty(details.ReturnValue))
        {
            lines.Add("/// <returns>");
            lines.Add($"/// <para>{ApiDocXmlCommentBuilder.CollapseToOneLine(details.ReturnValue!)}</para>");
            lines.Add("/// </returns>");
        }

        if (!string.IsNullOrEmpty(details.Remarks) || details.HelpLink is not null)
        {
            lines.Add("/// <remarks>");
            if (!string.IsNullOrEmpty(details.Remarks))
            {
                lines.Add($"/// <para>{ApiDocXmlCommentBuilder.CollapseToOneLine(details.Remarks!)}</para>");
            }

            if (details.HelpLink is not null)
            {
                lines.Add($"/// <para><see href=\"{details.HelpLink}#\">Read more on learn.microsoft.com</see>.</para>");
            }

            lines.Add("/// </remarks>");
        }

        return string.Join("\n", lines);
    }

    // the raw win32docs text is often split across multiple blank-line
    // separated paragraphs, but CsWin32's own doc comments run each field
    // as a single flowing paragraph - collapse internal line breaks (and
    // the blank lines between them) down to single spaces to match, rather
    // than emitting them as literal (uncommented, syntax-breaking) newlines
    // or as separate <para> elements CsWin32 itself doesn't produce here.
    private static string CollapseToOneLine(string text)
        => string.Join(
            " ",
            text.Replace("\r\n", "\n")
                .Split('\n')
                .Select(static line => line.Trim())
                .Where(static line => line.Length > 0));
}
