using FancyMouse.Win32Gen.Metadata;

namespace FancyMouse.Win32Gen.UnitTests.Metadata;

public static class Win32MetadataIndexTests
{
    [TestClass]
    public sealed class IndexMetadataTests
    {
        // one [TestMethod] per Win32MemberKind rather than [DynamicData],
        // since Win32MemberKind is internal and MSTest requires
        // [TestMethod] parameters to be public.
        [TestMethod]
        public void ClassifiesAnEnumBaseTypeAsEnum()
            => Win32MetadataIndexTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.EnumTypeName, nameof(Win32MemberKind.Enum));

        [TestMethod]
        public void ClassifiesAMulticastDelegateBaseTypeAsDelegate()
            => Win32MetadataIndexTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.DelegateTypeName, nameof(Win32MemberKind.Delegate));

        [TestMethod]
        public void ClassifiesANativeTypedefAttributeTypeAsStruct()
            => Win32MetadataIndexTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.StructTypeName, nameof(Win32MemberKind.Struct));

        [TestMethod]
        public void ClassifiesALiteralFieldOnApisAsConstant()
            => Win32MetadataIndexTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.ConstantFieldName, nameof(Win32MemberKind.Constant));

        [TestMethod]
        public void ClassifiesAPinvokeImplMethodOnApisAsFunction()
            => Win32MetadataIndexTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.FunctionMethodName, nameof(Win32MemberKind.Function));

        [TestMethod]
        public void UnknownNameIsNotIndexed()
        {
            var entriesByName = new Dictionary<string, Win32MemberKind>(StringComparer.Ordinal);
            Win32MetadataIndex.IndexMetadata(TestWinmdBuilder.Build(), entriesByName);

            Assert.IsFalse(entriesByName.ContainsKey("NotInTheMetadata"));
        }

        private static void AssertClassifiedAs(string name, string expectedKind)
        {
            var entriesByName = new Dictionary<string, Win32MemberKind>(StringComparer.Ordinal);
            Win32MetadataIndex.IndexMetadata(TestWinmdBuilder.Build(), entriesByName);

            Assert.IsTrue(entriesByName.TryGetValue(name, out var kind));
            Assert.AreEqual(expectedKind, kind.ToString());
        }
    }

    [TestClass]
    public sealed class TryClassifyTests
    {
        [TestMethod]
        public void ExactNameMatchIsClassified()
        {
            var index = Win32MetadataIndexTests.TryClassifyTests.FromEntries(("GetCursorPos", "Function"));

            Assert.IsTrue(index.TryClassify("GetCursorPos", out var kind));
            Assert.AreEqual("Function", kind.ToString());
        }

        [TestMethod]
        public void FallsBackToTheWSuffixedNameForAFunction()
        {
            // NativeMethods.txt (and this generator's own templates) often
            // name CsWin32's "friendly" overload ("DefWindowProc"), but the
            // raw metadata only has entries for the real 'W'-suffixed name.
            var index = Win32MetadataIndexTests.TryClassifyTests.FromEntries(("DefWindowProcW", "Function"));

            Assert.IsTrue(index.TryClassify("DefWindowProc", out var kind));
            Assert.AreEqual("Function", kind.ToString());
        }

        [TestMethod]
        public void FallsBackToTheASuffixedNameForAFunction()
        {
            var index = Win32MetadataIndexTests.TryClassifyTests.FromEntries(("SomeApiA", "Function"));

            Assert.IsTrue(index.TryClassify("SomeApi", out var kind));
            Assert.AreEqual("Function", kind.ToString());
        }

        [TestMethod]
        public void WSuffixFallbackOnlyAppliesToFunctions()
        {
            // the 'W'/'A' fallback exists because CsWin32 synthesizes
            // suffix-free "friendly overloads" only for functions - an enum
            // or constant literally named "FooW" is a distinct, real member,
            // not a stand-in for "Foo".
            var index = Win32MetadataIndexTests.TryClassifyTests.FromEntries(("FOO_ENUMW", "Enum"));

            Assert.IsFalse(index.TryClassify("FOO_ENUM", out _));
        }

        [TestMethod]
        public void UnknownNameIsNotClassified()
        {
            var index = Win32MetadataIndexTests.TryClassifyTests.FromEntries(("GetCursorPos", "Function"));

            Assert.IsFalse(index.TryClassify("NotInTheIndex", out _));
        }

        private static Win32MetadataIndex FromEntries(params (string Name, string Kind)[] entries)
        {
            var dictionary = new Dictionary<string, Win32MemberKind>(StringComparer.Ordinal);
            foreach (var (name, kind) in entries)
            {
                dictionary[name] = Enum.Parse<Win32MemberKind>(kind);
            }

            return Win32MetadataIndex.FromEntries(dictionary);
        }
    }
}
