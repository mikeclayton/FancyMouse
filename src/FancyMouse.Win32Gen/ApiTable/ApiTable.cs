using System.Collections.Immutable;

namespace FancyMouse.Win32Gen.ApiTable;

/// <summary>
/// The parsed, lookup-ready form of <c>ApiTable.txt</c> - see
/// <see cref="ApiTableHelper"/> for how a cached instance is obtained.
/// </summary>
internal sealed class ApiTable
{
    private readonly ImmutableDictionary<string, ApiEntry> entriesByApiName;

    public ApiTable(ImmutableDictionary<string, ApiEntry> entriesByApiName)
    {
        this.entriesByApiName = entriesByApiName;
    }

    public bool TryGet(string apiName, out ApiEntry entry)
        => this.entriesByApiName.TryGetValue(apiName, out entry!);
}
