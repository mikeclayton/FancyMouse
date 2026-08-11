namespace FancyMouse.Win32Gen.ApiTable;

/// <summary>
/// The parsed form of an <c>ApiTable.txt</c> bracketed tag (e.g.
/// <c>[SuccessIsNonZero]</c>) - mirrors the declarative attribute types in
/// <c>Source\Attributes\*.cs</c> conceptually, even where the table's own
/// tag spelling doesn't match the attribute type name exactly (the table's
/// <c>[UsesLastError]</c> is <see cref="UseLastError"/> here, matching
/// <c>UseLastErrorAttribute</c>; <c>[SuccessIsCustom]</c> is
/// <see cref="SuccessDelegate"/>, matching <c>SuccessDelegateAttribute</c>).
/// </summary>
internal enum ApiAttributeKind
{
    SuccessIsNonZero,
    SuccessIsNotNull,
    AlwaysSucceeds,

    // placeholder only, for now - no delegate method name is parsed or
    // carried yet (see ApiTableParser).
    SuccessDelegate,

    UseLastError,
    HumanVerified,
}
