namespace FancyMouse.Win32Gen.ApiDocs;

/// <summary>
/// Mirrors CsWin32's own internal <c>ApiDetails</c> record shape (see
/// https://github.com/microsoft/CsWin32/blob/main/src/Microsoft.Windows.CsWin32/Docs.cs) -
/// not something this assembly can reference directly, since it's internal
/// to CsWin32's own analyzer assembly, so it's replicated here purely as a
/// deserialization target for the same win32docs data file.
/// </summary>
internal sealed class ApiDetails
{
    public Uri? HelpLink
    {
        get;
        set;
    }

    public string? Description
    {
        get;
        set;
    }

    public string? Remarks
    {
        get;
        set;
    }

    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.Ordinal);

    public Dictionary<string, string> Fields { get; set; } = new(StringComparer.Ordinal);

    public string? ReturnValue
    {
        get;
        set;
    }
}
