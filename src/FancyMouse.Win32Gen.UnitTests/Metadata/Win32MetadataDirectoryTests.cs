using FancyMouse.Win32Gen.Metadata;

namespace FancyMouse.Win32Gen.UnitTests.Metadata;

public static class Win32MetadataDirectoryTests
{
    [TestClass]
    public sealed class IndexMetadataTests
    {
        // one [TestMethod] per Win32MemberKind rather than [DynamicData],
        // since Win32MemberKind is internal and MSTest requires
        // [TestMethod] parameters to be public.
        [TestMethod]
        public void ClassifiesAnEnumBaseTypeAsEnum()
            => Win32MetadataDirectoryTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.EnumTypeName, nameof(Win32MemberKind.Enum));

        [TestMethod]
        public void ClassifiesAMulticastDelegateBaseTypeAsDelegate()
            => Win32MetadataDirectoryTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.DelegateTypeName, nameof(Win32MemberKind.Delegate));

        [TestMethod]
        public void ClassifiesANativeTypedefAttributeTypeAsStruct()
            => Win32MetadataDirectoryTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.StructTypeName, nameof(Win32MemberKind.Struct));

        [TestMethod]
        public void ClassifiesALiteralFieldOnApisAsConstant()
            => Win32MetadataDirectoryTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.ConstantFieldName, nameof(Win32MemberKind.Constant));

        [TestMethod]
        public void ClassifiesAPinvokeImplMethodOnApisAsFunction()
            => Win32MetadataDirectoryTests.IndexMetadataTests.AssertClassifiedAs(TestWinmdBuilder.FunctionMethodName, nameof(Win32MemberKind.Function));

        [TestMethod]
        public void UnknownNameIsNotIndexed()
        {
            var entriesByName = new Dictionary<string, Win32MetadataEntry>(StringComparer.Ordinal);
            Win32MetadataDirectory.IndexMetadata(TestWinmdBuilder.Build(), entriesByName);

            Assert.IsFalse(entriesByName.ContainsKey("NotInTheMetadata"));
        }

        private static void AssertClassifiedAs(string name, string expectedKind)
        {
            var entriesByName = new Dictionary<string, Win32MetadataEntry>(StringComparer.Ordinal);
            Win32MetadataDirectory.IndexMetadata(TestWinmdBuilder.Build(), entriesByName);

            Assert.IsTrue(entriesByName.TryGetValue(name, out var entry));
            Assert.AreEqual(name, entry.Name);
            Assert.AreEqual(expectedKind, entry.Kind.ToString());
        }
    }

    [TestClass]
    public sealed class TryGetTests
    {
        [TestMethod]
        public void ExactNameMatchIsResolved()
        {
            var directory = Win32MetadataDirectoryTests.TryGetTests.FromEntries(("GetCursorPos", "Function"));

            Assert.IsTrue(directory.TryGet("GetCursorPos", out var entry));
            Assert.AreEqual("GetCursorPos", entry!.Name);
            Assert.AreEqual("Function", entry.Kind.ToString());
        }

        [TestMethod]
        public void FallsBackToTheWSuffixedNameForAFunction()
        {
            // NativeMethods.txt (and this generator's own templates) often
            // name CsWin32's "friendly" overload ("DefWindowProc"), but the
            // raw metadata only has entries for the real 'W'-suffixed name.
            var directory = Win32MetadataDirectoryTests.TryGetTests.FromEntries(("DefWindowProcW", "Function"));

            Assert.IsTrue(directory.TryGet("DefWindowProc", out var entry));
            Assert.AreEqual("Function", entry!.Kind.ToString());

            // the returned entry's own name - not the name TryGet was
            // called with - tells the caller it was the 'W'-suffixed
            // fallback that actually matched.
            Assert.AreEqual("DefWindowProcW", entry.Name);
        }

        [TestMethod]
        public void FallsBackToTheASuffixedNameForAFunction()
        {
            var directory = Win32MetadataDirectoryTests.TryGetTests.FromEntries(("SomeApiA", "Function"));

            Assert.IsTrue(directory.TryGet("SomeApi", out var entry));
            Assert.AreEqual("Function", entry!.Kind.ToString());
            Assert.AreEqual("SomeApiA", entry.Name);
        }

        [TestMethod]
        public void WSuffixFallbackOnlyAppliesToFunctions()
        {
            // the 'W'/'A' fallback exists because CsWin32 synthesizes
            // suffix-free "friendly overloads" only for functions - an enum
            // or constant literally named "FooW" is a distinct, real member,
            // not a stand-in for "Foo".
            var directory = Win32MetadataDirectoryTests.TryGetTests.FromEntries(("FOO_ENUMW", "Enum"));

            Assert.IsFalse(directory.TryGet("FOO_ENUM", out _));
        }

        [TestMethod]
        public void UnknownNameIsNotResolved()
        {
            var directory = Win32MetadataDirectoryTests.TryGetTests.FromEntries(("GetCursorPos", "Function"));

            Assert.IsFalse(directory.TryGet("NotInTheIndex", out _));
        }

        // seeds the name -> entry index directly, bypassing metadata
        // loading entirely - lets TryGet's name/'W'/'A' fallback logic be
        // tested with an inline dictionary instead of a real .winmd file.
        private static Win32MetadataDirectory FromEntries(params (string Name, string Kind)[] entries)
        {
            var entriesByName = new Dictionary<string, Win32MetadataEntry>(StringComparer.Ordinal);
            foreach (var (name, kind) in entries)
            {
                entriesByName[name] = new Win32MetadataEntry(name, Enum.Parse<Win32MemberKind>(kind));
            }

            return new Win32MetadataDirectory(entriesByName);
        }
    }
}
