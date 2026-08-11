using FancyMouse.Win32Gen.ApiTable;

namespace FancyMouse.Win32Gen.UnitTests.ApiTable;

public static class ApiTableParserTests
{
    [TestClass]
    public sealed class ParseTests
    {
        // MSTest requires [TestMethod] parameters to be public, so this
        // can't carry ApiAttributeKind (internal) directly - the parsed
        // entry's attributes are compared by name (ToString()) instead.
        public sealed class TestCase
        {
            public TestCase(string input, string expectedApiName, params string[] expectedAttributeKinds)
            {
                this.Input = input;
                this.ExpectedApiName = expectedApiName;
                this.ExpectedAttributeKinds = expectedAttributeKinds;
            }

            public string Input { get; }

            public string ExpectedApiName { get; }

            public string[] ExpectedAttributeKinds { get; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            // a plain api name with no tags produces an entry with no
            // attributes
            yield return new object[]
            {
                new TestCase("GetCursorPos", "GetCursorPos"),
            };

            // a single tag is parsed and attached to the api name that
            // follows it
            yield return new object[]
            {
                new TestCase("[SuccessIsNonZero] AppendMenu", "AppendMenu", nameof(ApiAttributeKind.SuccessIsNonZero)),
            };

            // multiple tags on one line are all attached, in the order they
            // appear
            yield return new object[]
            {
                new TestCase(
                    "[SuccessIsNonZero] [UsesLastError] AppendMenu",
                    "AppendMenu",
                    nameof(ApiAttributeKind.SuccessIsNonZero),
                    nameof(ApiAttributeKind.UseLastError)),
            };

            // every recognised tag maps to its corresponding
            // ApiAttributeKind - including the two whose tag spelling
            // doesn't match the enum member name (UsesLastError ->
            // UseLastError, SuccessIsCustom -> SuccessDelegate)
            yield return new object[]
            {
                new TestCase("[SuccessIsNonZero] Api1", "Api1", nameof(ApiAttributeKind.SuccessIsNonZero)),
            };
            yield return new object[]
            {
                new TestCase("[SuccessIsNotNull] Api2", "Api2", nameof(ApiAttributeKind.SuccessIsNotNull)),
            };
            yield return new object[]
            {
                new TestCase("[AlwaysSucceeds] Api3", "Api3", nameof(ApiAttributeKind.AlwaysSucceeds)),
            };
            yield return new object[]
            {
                new TestCase("[SuccessIsCustom] Api4", "Api4", nameof(ApiAttributeKind.SuccessDelegate)),
            };
            yield return new object[]
            {
                new TestCase("[UsesLastError] Api5", "Api5", nameof(ApiAttributeKind.UseLastError)),
            };
            yield return new object[]
            {
                new TestCase("[HumanVerified] Api6", "Api6", nameof(ApiAttributeKind.HumanVerified)),
            };

            // a realistic multi-line file combining comments, a blank line,
            // and several tagged/untagged entries, to confirm each line is
            // parsed independently
            yield return new object[]
            {
                new TestCase(
                    """
                    // ApiTable.txt
                    [SuccessIsNonZero] [UsesLastError] AppendMenu

                    GetCursorPos
                    """,
                    "AppendMenu",
                    nameof(ApiAttributeKind.SuccessIsNonZero),
                    nameof(ApiAttributeKind.UseLastError)),
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases))]
        public void RunTestCases(TestCase data)
        {
            var table = ApiTableParser.Parse(data.Input);

            var found = table.TryGet(data.ExpectedApiName, out var entry);

            Assert.IsTrue(found);
            Assert.AreEqual(data.ExpectedApiName, entry.ApiName);
            CollectionAssert.AreEqual(
                data.ExpectedAttributeKinds,
                entry.Attributes.Select(kind => kind.ToString()).ToArray());
        }

        [TestMethod]
        public void CommentAndBlankLinesProduceNoEntries()
        {
            const string input = """
                // this is a comment

                GetCursorPos
                """;

            var table = ApiTableParser.Parse(input);

            Assert.IsFalse(table.TryGet(string.Empty, out _));
            Assert.IsFalse(table.TryGet("//", out _));
            Assert.IsTrue(table.TryGet("GetCursorPos", out _));
        }

        [TestMethod]
        public void UnknownApiNameIsNotFound()
        {
            var table = ApiTableParser.Parse("GetCursorPos");

            Assert.IsFalse(table.TryGet("SetCursorPos", out _));
        }

        [TestMethod]
        public void UnrecognizedTagThrowsFormatException()
            => Assert.ThrowsExactly<FormatException>(() => ApiTableParser.Parse("[NotARealTag] GetCursorPos"));

        [TestMethod]
        public void MissingApiNameThrowsFormatException()
            => Assert.ThrowsExactly<FormatException>(() => ApiTableParser.Parse("[SuccessIsNonZero]"));
    }
}
