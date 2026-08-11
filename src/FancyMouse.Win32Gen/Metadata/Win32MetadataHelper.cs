using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace FancyMouse.Win32Gen.Metadata;

/// <summary>
/// Walks a win32metadata .winmd file collecting the return type of every
/// P/Invoke function named in a given set of function names - used to work
/// out, for a given NativeMethods.txt, exactly which CsWin32-generated
/// return types (<c>BOOL</c>, <c>HWND</c>, <c>HMENU</c>, ...) that project's
/// own wrappers will actually need, so only the matching
/// <c>Win32ReturnCode_*.cs</c> framework partials get emitted for it.
/// </summary>
/// <remarks>
/// Shares the same "Apis" P/Invoke-function shape and 'W'/'A' friendly-name
/// fallback as <see cref="Win32MetadataIndex"/>, but that type only keeps a
/// name -&gt; <see cref="Win32MemberKind"/> classification and discards the
/// method itself, so it can't answer "what does this function return" -
/// hence a separate walk here rather than extending that index.
/// </remarks>
internal static class Win32MetadataHelper
{
    private const string ApisTypeName = "Apis";

    public static ImmutableHashSet<string> GetReturnTypes(
        IEnumerable<string> functionNames,
        ImmutableArray<string> winmdPaths)
    {
        var names = new HashSet<string>(functionNames, StringComparer.Ordinal);
        var returnTypes = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (var path in winmdPaths)
        {
            Win32MetadataHelper.CollectReturnTypes(path, names, returnTypes);
        }

        return returnTypes.ToImmutable();
    }

    private static void CollectReturnTypes(
        string winmdPath,
        HashSet<string> functionNames,
        ImmutableHashSet<string>.Builder returnTypes)
    {
        if (!File.Exists(winmdPath))
        {
            return;
        }

        try
        {
            using var stream = File.OpenRead(winmdPath);
            using var peReader = new PEReader(stream);
            var reader = peReader.GetMetadataReader();
            var provider = new Win32MetadataHelper.ReturnTypeProvider();

            foreach (var handle in reader.MethodDefinitions)
            {
                var method = reader.GetMethodDefinition(handle);
                if ((method.Attributes & MethodAttributes.PinvokeImpl) == 0
                    || !Win32MetadataHelper.IsApisType(reader, method.GetDeclaringType()))
                {
                    continue;
                }

                var name = reader.GetString(method.Name);
                if (!Win32MetadataHelper.MatchesRequestedName(name, functionNames))
                {
                    continue;
                }

                var signature = method.DecodeSignature(provider, genericContext: null);
                if (!string.IsNullOrEmpty(signature.ReturnType) && signature.ReturnType != "Void")
                {
                    returnTypes.Add(signature.ReturnType);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or BadImageFormatException or UnauthorizedAccessException)
        {
            // metadata-based return-type discovery is a nice-to-have, not
            // something a build should fail over - just skip this file.
        }
    }

    // NativeMethods.txt often names the CsWin32 "friendly" overload (e.g.
    // "DefWindowProc"), but only the 'W'/'A'-suffixed name exists as a raw
    // metadata MethodDefinition - same gap noted on
    // Win32MetadataIndex.TryClassify.
    private static bool MatchesRequestedName(string metadataName, HashSet<string> functionNames)
    {
        if (functionNames.Contains(metadataName))
        {
            return true;
        }

        return metadataName.Length > 1
            && (metadataName[metadataName.Length - 1] == 'W' || metadataName[metadataName.Length - 1] == 'A')
            && functionNames.Contains(metadataName.Substring(0, metadataName.Length - 1));
    }

    private static bool IsApisType(MetadataReader reader, TypeDefinitionHandle handle)
        => reader.GetString(reader.GetTypeDefinition(handle).Name) == Win32MetadataHelper.ApisTypeName;

    /// <summary>
    /// Minimal <see cref="ISignatureTypeProvider{TType, TGenericContext}"/> -
    /// only needs to resolve enough of a method signature to name the return
    /// type; every other signature shape it decodes along the way (pointers,
    /// arrays, generics, ...) is discarded, so it's deliberately permissive
    /// rather than a complete type-name resolver.
    /// </summary>
    private sealed class ReturnTypeProvider : ISignatureTypeProvider<string, object?>
    {
        public string GetPrimitiveType(PrimitiveTypeCode typeCode)
            => typeCode.ToString();

        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
            => reader.GetString(reader.GetTypeDefinition(handle).Name);

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            => reader.GetString(reader.GetTypeReference(handle).Name);

        public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
            => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

        public string GetSZArrayType(string elementType)
            => elementType + "[]";

        public string GetArrayType(string elementType, ArrayShape shape)
            => elementType + "[]";

        public string GetByReferenceType(string elementType)
            => elementType;

        public string GetPointerType(string elementType)
            => elementType + "*";

        public string GetFunctionPointerType(MethodSignature<string> signature)
            => "IntPtr";

        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
            => genericType;

        public string GetGenericMethodParameter(object? genericContext, int index)
            => "T";

        public string GetGenericTypeParameter(object? genericContext, int index)
            => "T";

        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired)
            => unmodifiedType;

        public string GetPinnedType(string elementType)
            => elementType;
    }
}
