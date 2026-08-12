using FancyMouse.Win32Gen.NativeMethods;

namespace FancyMouse.Win32Gen.UnitTests.NativeMethods;

public static class NativeMethodsTxtParserTests
{
    [TestClass]
    public sealed class ParseTests
    {
        // MSTest requires [TestMethod] parameters to be public, so this
        // can't carry NativeMethodsEntryKind (internal) directly - the
        // parsed entry's Kind is compared by name (ToString()) instead.
        public sealed class TestCase
        {
            public TestCase(string input, params (string Kind, string Name)[] expected)
            {
                this.Input = input;
                this.Expected = expected;
            }

            public string Input { get; }

            public (string Kind, string Name)[] Expected { get; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            // a plain api name line produces an ApiName entry, verbatim
            yield return new object[]
            {
                new TestCase(
                    "GetCursorPos",
                    (nameof(NativeMethodsEntryKind.ApiName), "GetCursorPos")),
            };

            // a "Module.*" line produces a ModuleWildcard entry with the
            // ".*" suffix stripped
            yield return new object[]
            {
                new TestCase(
                    "User32.*",
                    (nameof(NativeMethodsEntryKind.ModuleWildcard), "User32")),
            };

            // a "-Name" line produces an Exclusion entry with the "-"
            // prefix stripped
            yield return new object[]
            {
                new TestCase(
                    "-GetCursorPos",
                    (nameof(NativeMethodsEntryKind.Exclusion), "GetCursorPos")),
            };

            // a "//" comment line produces no entries at all
            yield return new object[]
            {
                new TestCase(
                    "// this is a comment",
                    Array.Empty<(string, string)>()),
            };

            // a realistic multi-line file combining every entry kind and a
            // comment, to confirm each line is parsed independently and in
            // order
            yield return new object[]
            {
                new TestCase(
                    """
                    // NativeMethods.txt
                    GetCursorPos
                    User32.*
                    -SetCursorPos
                    """,
                    (nameof(NativeMethodsEntryKind.ApiName), "GetCursorPos"),
                    (nameof(NativeMethodsEntryKind.ModuleWildcard), "User32"),
                    (nameof(NativeMethodsEntryKind.Exclusion), "SetCursorPos")),
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases))]
        public void RunTestCases(TestCase data)
        {
            var file = new TestAdditionalText(data.Input);
            var actual = NativeMethodsTxtParser.Parse(file, CancellationToken.None).Entries
                .Select(entry => (Kind: entry.Kind.ToString(), entry.Name))
                .ToArray();
            CollectionAssert.AreEqual(data.Expected, actual);
        }

        [TestMethod]
        public void EachEntryLocationSpansItsOwnSourceLine()
        {
            const string input = "GetCursorPos\n-SetCursorPos\nUser32.*\n";
            var expectedLines = new[] { "GetCursorPos", "-SetCursorPos", "User32.*" };

            var file = new TestAdditionalText(input);
            var entries = NativeMethodsTxtParser.Parse(file, CancellationToken.None).Entries;

            Assert.HasCount(expectedLines.Length, entries);
            for (var i = 0; i < entries.Count; i++)
            {
                var span = entries[i].Location.SourceSpan;
                var lineText = input.Substring(span.Start, span.Length);
                Assert.AreEqual(expectedLines[i], lineText);
            }
        }
    }
}
