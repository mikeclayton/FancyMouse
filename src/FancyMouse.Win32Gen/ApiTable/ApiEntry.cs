using System.Collections.Immutable;

namespace FancyMouse.Win32Gen.ApiTable;

/// <summary>
/// One parsed line of <c>ApiTable.txt</c> - an api name and the set of
/// declarative attributes recorded against it.
/// </summary>
internal sealed record ApiEntry(string ApiName, ImmutableArray<ApiAttributeKind> Attributes)
{
    public bool Has(ApiAttributeKind kind)
        => this.Attributes.Contains(kind);
}
