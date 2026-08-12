using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace FancyMouse.Win32Gen.Metadata;

/// <summary>
/// Reads the raw ECMA-335 metadata tables of a win32metadata .winmd file
/// (the same file CsWin32 itself consumes - see
/// https://github.com/microsoft/win32metadata) and builds a name -&gt;
/// <see cref="Win32MetadataEntry"/> index, so this generator can tell an
/// enum/constant apart from a function without depending on CsWin32's own
/// (private, unexported) resolution logic.
/// </summary>
/// <remarks>
/// Deliberately coarse - no signature resolution, no handling of the
/// occasional real ambiguity between kinds - good enough for classifying a
/// NativeMethods.txt entry for diagnostics, nowhere near enough to
/// generate a P/Invoke signature.
///
/// Empirically, per-header schema (verified directly against
/// Windows.Win32.winmd 69.0.7-preview, not just CsWin32's docs):
/// - enums are TypeDefinitions whose base type is System.Enum;
/// - global constants are Literal FieldDefinitions declared on a
///   type literally named "Apis" (one such type per header/namespace,
///   e.g. Windows.Win32.Foundation.Apis);
/// - P/Invoke functions are MethodDefinitions with the PinvokeImpl
///   attribute, also declared on an "Apis" type - the DLL name is
///   available via the method's ImplMap/ModuleRef if ever needed;
/// - "native typedef" structs (handles like HWND/HBRUSH, and primitives
///   like BOOL) are TypeDefinitions carrying a NativeTypedefAttribute
///   custom attribute;
/// - function pointer typedefs (e.g. WNDPROC) are TypeDefinitions whose
///   base type is System.MulticastDelegate - a different shape from
///   native-typedef structs, with no NativeTypedefAttribute of their own.
///
/// One important gap: CsWin32 synthesizes suffix-free "friendly overloads"
/// for most ANSI/Unicode ('W'/'A') function pairs - e.g. "DefWindowProc" is
/// a real, generatable CsWin32 name, but only "DefWindowProcW"/"DefWindowProcA"
/// exist as raw metadata MethodDefinitions. <see cref="TryGet"/> falls
/// back to the 'W'/'A'-suffixed name for exactly this reason.
/// </remarks>
internal sealed class Win32MetadataDirectory
{
    private const string ApisTypeName = "Apis";
    private const string EnumBaseTypeFullName = "System.Enum";
    private const string DelegateBaseTypeFullName = "System.MulticastDelegate";
    private const string NativeTypedefAttributeFullName = "Windows.Win32.Foundation.Metadata.NativeTypedefAttribute";

    private readonly Dictionary<string, Win32MetadataEntry> entriesByName;

    // internal rather than private: the unit tests build a directory
    // directly from an in-memory entriesByName dictionary, bypassing
    // Load's real .winmd file reading, so TryGet's name/'W'/'A' fallback
    // logic can be tested without a real .winmd file.
    internal Win32MetadataDirectory(Dictionary<string, Win32MetadataEntry> entriesByName)
    {
        this.entriesByName = entriesByName;
    }

    public static Win32MetadataDirectory? Load(ImmutableArray<string> winmdPaths)
    {
        if (winmdPaths.IsDefaultOrEmpty)
        {
            return null;
        }

        var entriesByName = new Dictionary<string, Win32MetadataEntry>(StringComparer.Ordinal);
        foreach (var path in winmdPaths)
        {
            Win32MetadataDirectory.TryLoadInto(path, entriesByName);
        }

        return new Win32MetadataDirectory(entriesByName);
    }

    /// <summary>
    /// Resolves <paramref name="name"/> to the <see cref="Win32MetadataEntry"/>
    /// it names - falling back to the 'W'/'A'-suffixed function name CsWin32
    /// itself would have resolved, when the un-suffixed name has no direct
    /// metadata entry of its own. The returned entry's own <see cref="Win32MetadataEntry.Name"/>
    /// lets a caller tell whether the exact name matched, or which suffixed
    /// fallback did.
    /// </summary>
    public bool TryGet(string name, out Win32MetadataEntry? entry)
    {
        if (this.entriesByName.TryGetValue(name, out entry!))
        {
            return true;
        }

        if (this.entriesByName.TryGetValue(name + "W", out entry) && entry.Kind == Win32MemberKind.Function)
        {
            return true;
        }

        if (this.entriesByName.TryGetValue(name + "A", out entry) && entry.Kind == Win32MemberKind.Function)
        {
            return true;
        }

        entry = null;
        return false;
    }

    private static void TryLoadInto(string winmdPath, Dictionary<string, Win32MetadataEntry> entriesByName)
    {
        if (!File.Exists(winmdPath))
        {
            return;
        }

        try
        {
            using var stream = File.OpenRead(winmdPath);
            using var peReader = new PEReader(stream);
            Win32MetadataDirectory.IndexMetadata(peReader.GetMetadataReader(), entriesByName);
        }
        catch (Exception ex) when (ex is IOException or BadImageFormatException or UnauthorizedAccessException)
        {
            // metadata-based classification is a nice-to-have, not something
            // a build should fail over - just skip this file.
        }
    }

    /// <summary>
    /// Walks every TypeDefinition/FieldDefinition/MethodDefinition in
    /// <paramref name="reader"/> and classifies each by
    /// <see cref="Win32MemberKind"/>. Split out of <see cref="TryLoadInto"/>
    /// (which pairs this with opening the real .winmd file as a PE) purely
    /// so it can be exercised directly against a small, in-memory-built
    /// <see cref="MetadataReader"/> in tests, instead of a real
    /// multi-megabyte win32metadata .winmd file.
    /// </summary>
    internal static void IndexMetadata(MetadataReader reader, Dictionary<string, Win32MetadataEntry> entriesByName)
    {
        foreach (var handle in reader.TypeDefinitions)
        {
            var type = reader.GetTypeDefinition(handle);
            var baseTypeName = type.BaseType.IsNil ? string.Empty : Win32MetadataDirectory.ResolveTypeName(reader, type.BaseType);

            if (baseTypeName == Win32MetadataDirectory.EnumBaseTypeFullName)
            {
                Win32MetadataDirectory.AddIfAbsent(entriesByName, reader.GetString(type.Name), Win32MemberKind.Enum);
            }
            else if (baseTypeName == Win32MetadataDirectory.DelegateBaseTypeFullName)
            {
                Win32MetadataDirectory.AddIfAbsent(entriesByName, reader.GetString(type.Name), Win32MemberKind.Delegate);
            }
            else if (Win32MetadataDirectory.HasCustomAttribute(reader, type, Win32MetadataDirectory.NativeTypedefAttributeFullName))
            {
                Win32MetadataDirectory.AddIfAbsent(entriesByName, reader.GetString(type.Name), Win32MemberKind.Struct);
            }
        }

        foreach (var handle in reader.FieldDefinitions)
        {
            var field = reader.GetFieldDefinition(handle);
            if ((field.Attributes & FieldAttributes.Literal) != 0
                && Win32MetadataDirectory.IsApisType(reader, field.GetDeclaringType()))
            {
                Win32MetadataDirectory.AddIfAbsent(entriesByName, reader.GetString(field.Name), Win32MemberKind.Constant);
            }
        }

        foreach (var handle in reader.MethodDefinitions)
        {
            var method = reader.GetMethodDefinition(handle);
            if ((method.Attributes & MethodAttributes.PinvokeImpl) != 0
                && Win32MetadataDirectory.IsApisType(reader, method.GetDeclaringType()))
            {
                Win32MetadataDirectory.AddIfAbsent(entriesByName, reader.GetString(method.Name), Win32MemberKind.Function);
            }
        }
    }

    private static void AddIfAbsent(Dictionary<string, Win32MetadataEntry> entriesByName, string name, Win32MemberKind kind)
    {
        if (!entriesByName.ContainsKey(name))
        {
            entriesByName[name] = new Win32MetadataEntry(name, kind);
        }
    }

    private static bool IsApisType(MetadataReader reader, TypeDefinitionHandle handle)
        => reader.GetString(reader.GetTypeDefinition(handle).Name) == Win32MetadataDirectory.ApisTypeName;

    private static bool HasCustomAttribute(MetadataReader reader, TypeDefinition type, string attributeFullName)
    {
        foreach (var handle in type.GetCustomAttributes())
        {
            var attribute = reader.GetCustomAttribute(handle);
            if (Win32MetadataDirectory.ResolveAttributeTypeName(reader, attribute.Constructor) == attributeFullName)
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveAttributeTypeName(MetadataReader reader, EntityHandle constructorHandle)
    {
        switch (constructorHandle.Kind)
        {
            case HandleKind.MemberReference:
                var memberRef = reader.GetMemberReference((MemberReferenceHandle)constructorHandle);
                return Win32MetadataDirectory.ResolveTypeName(reader, memberRef.Parent);

            case HandleKind.MethodDefinition:
                var methodDef = reader.GetMethodDefinition((MethodDefinitionHandle)constructorHandle);
                return Win32MetadataDirectory.ResolveTypeName(reader, methodDef.GetDeclaringType());

            default:
                return string.Empty;
        }
    }

    private static string ResolveTypeName(MetadataReader reader, EntityHandle handle)
    {
        switch (handle.Kind)
        {
            case HandleKind.TypeReference:
                var typeRef = reader.GetTypeReference((TypeReferenceHandle)handle);
                return $"{reader.GetString(typeRef.Namespace)}.{reader.GetString(typeRef.Name)}";

            case HandleKind.TypeDefinition:
                var typeDef = reader.GetTypeDefinition((TypeDefinitionHandle)handle);
                return $"{reader.GetString(typeDef.Namespace)}.{reader.GetString(typeDef.Name)}";

            default:
                return string.Empty;
        }
    }
}
