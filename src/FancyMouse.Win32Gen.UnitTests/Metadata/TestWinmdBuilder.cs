using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace FancyMouse.Win32Gen.UnitTests.Metadata;

/// <summary>
/// Builds a small, in-memory ECMA-335 metadata blob shaped like a real
/// win32metadata .winmd file - just enough of each construct
/// <see cref="Win32Gen.Metadata.Win32MetadataDirectory.IndexMetadata"/> looks
/// for (an enum, a delegate, a native-typedef struct, and an "Apis" type
/// carrying a literal constant field and a PinvokeImpl method) - so tests
/// don't need to load a real, multi-megabyte win32metadata file to exercise
/// the classification logic.
/// </summary>
internal static class TestWinmdBuilder
{
    public const string EnumTypeName = "SOME_ENUM";
    public const string DelegateTypeName = "SOME_DELEGATE";
    public const string StructTypeName = "HSOMETHING";
    public const string ConstantFieldName = "SOME_CONSTANT";
    public const string FunctionMethodName = "SomeFunctionW";

    public static MetadataReader Build()
    {
        var metadata = new MetadataBuilder();

        metadata.AddModule(
            generation: 0,
            moduleName: metadata.GetOrAddString("TestWinmd.winmd"),
            mvid: metadata.GetOrAddGuid(Guid.NewGuid()),
            encId: default,
            encBaseId: default);

        metadata.AddAssembly(
            name: metadata.GetOrAddString("TestWinmd"),
            version: new Version(1, 0, 0, 0),
            culture: default,
            publicKey: default,
            flags: default,
            hashAlgorithm: AssemblyHashAlgorithm.None);

        var corlibRef = metadata.AddAssemblyReference(
            name: metadata.GetOrAddString("mscorlib"),
            version: new Version(4, 0, 0, 0),
            culture: default,
            publicKeyOrToken: default,
            flags: default,
            hashValue: default);

        TypeReferenceHandle AddTypeRef(string ns, string name)
            => metadata.AddTypeReference(corlibRef, metadata.GetOrAddString(ns), metadata.GetOrAddString(name));

        var objectRef = AddTypeRef("System", "Object");
        var enumRef = AddTypeRef("System", "Enum");
        var delegateRef = AddTypeRef("System", "MulticastDelegate");
        var valueTypeRef = AddTypeRef("System", "ValueType");
        var nativeTypedefAttributeRef = AddTypeRef("Windows.Win32.Foundation.Metadata", "NativeTypedefAttribute");

        var ctorSigBuilder = new BlobBuilder();
        new BlobEncoder(ctorSigBuilder)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(0, returnType => returnType.Void(), _ => { });
        var attributeCtor = metadata.AddMemberReference(
            nativeTypedefAttributeRef,
            metadata.GetOrAddString(".ctor"),
            metadata.GetOrAddBlob(ctorSigBuilder));

        // <Module> - the mandatory first TypeDef row. Every type added
        // below (until Apis) owns no fields/methods of its own, so each
        // just points forward to the next field/method row that hasn't
        // been added yet - the standard ECMA-335 pattern for empty ranges.
        var nextField = MetadataTokens.FieldDefinitionHandle(1);
        var nextMethod = MetadataTokens.MethodDefinitionHandle(1);

        metadata.AddTypeDefinition(default, default, metadata.GetOrAddString("<Module>"), default, nextField, nextMethod);

        metadata.AddTypeDefinition(
            TypeAttributes.Public,
            default,
            metadata.GetOrAddString(TestWinmdBuilder.EnumTypeName),
            enumRef,
            nextField,
            nextMethod);

        metadata.AddTypeDefinition(
            TypeAttributes.Public,
            default,
            metadata.GetOrAddString(TestWinmdBuilder.DelegateTypeName),
            delegateRef,
            nextField,
            nextMethod);

        var structHandle = metadata.AddTypeDefinition(
            TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.Sealed,
            default,
            metadata.GetOrAddString(TestWinmdBuilder.StructTypeName),
            valueTypeRef,
            nextField,
            nextMethod);

        var attributeValue = new BlobBuilder();
        attributeValue.WriteUInt16(1); // prolog
        attributeValue.WriteUInt16(0); // no named arguments
        metadata.AddCustomAttribute(structHandle, attributeCtor, metadata.GetOrAddBlob(attributeValue));

        metadata.AddTypeDefinition(
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed,
            default,
            metadata.GetOrAddString("Apis"),
            objectRef,
            nextField,
            nextMethod);

        var fieldSigBuilder = new BlobBuilder();
        new BlobEncoder(fieldSigBuilder).FieldSignature().Int32();
        var constantField = metadata.AddFieldDefinition(
            FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
            metadata.GetOrAddString(TestWinmdBuilder.ConstantFieldName),
            metadata.GetOrAddBlob(fieldSigBuilder));
        metadata.AddConstant(constantField, 42);

        var methodSigBuilder = new BlobBuilder();
        new BlobEncoder(methodSigBuilder)
            .MethodSignature(isInstanceMethod: false)
            .Parameters(0, returnType => returnType.Void(), _ => { });
        var moduleRef = metadata.AddModuleReference(metadata.GetOrAddString("SOMEDLL.dll"));
        var functionMethod = metadata.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig,
            MethodImplAttributes.PreserveSig,
            metadata.GetOrAddString(TestWinmdBuilder.FunctionMethodName),
            metadata.GetOrAddBlob(methodSigBuilder),
            bodyOffset: -1,
            parameterList: MetadataTokens.ParameterHandle(1));
        metadata.AddMethodImport(
            functionMethod,
            MethodImportAttributes.CallingConventionWinApi,
            metadata.GetOrAddString(TestWinmdBuilder.FunctionMethodName),
            moduleRef);

        var rootBuilder = new MetadataRootBuilder(metadata);
        var imageBuilder = new BlobBuilder();
        rootBuilder.Serialize(imageBuilder, methodBodyStreamRva: 0, mappedFieldDataStreamRva: 0);

        var provider = MetadataReaderProvider.FromMetadataImage(imageBuilder.ToImmutableArray());
        return provider.GetMetadataReader();
    }
}
