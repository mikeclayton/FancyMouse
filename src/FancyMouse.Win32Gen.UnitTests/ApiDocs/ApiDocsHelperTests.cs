using System.Buffers;

using FancyMouse.Win32Gen.ApiDocs;

using MessagePack;

namespace FancyMouse.Win32Gen.UnitTests.ApiDocs;

public static class ApiDocsHelperTests
{
    [TestClass]
    public sealed class GetXmlDocsForFunctionTests
    {
        [TestMethod]
        public void ReturnsRenderedDocsForAnExactNameMatch()
        {
            var helper = ApiDocsHelper.FromEntries(new Dictionary<string, ApiDetails>(StringComparer.Ordinal)
            {
                ["GetCursorPos"] = new() { Description = "Retrieves the cursor's position." },
            });

            var actual = helper.GetXmlDocsForFunction("GetCursorPos");

            Assert.AreEqual("/// <summary>Retrieves the cursor's position.</summary>", actual);
        }

        [TestMethod]
        public void FallsBackToTheWSuffixedName()
        {
            // NativeMethods.txt (and this generator's own templates) often
            // name CsWin32's "friendly" overload ("DefWindowProc"), but the
            // docs file only has entries for the real 'W'-suffixed name.
            var helper = ApiDocsHelper.FromEntries(new Dictionary<string, ApiDetails>(StringComparer.Ordinal)
            {
                ["DefWindowProcW"] = new() { Description = "Calls the default window procedure." },
            });

            var actual = helper.GetXmlDocsForFunction("DefWindowProc");

            Assert.AreEqual("/// <summary>Calls the default window procedure.</summary>", actual);
        }

        [TestMethod]
        public void FallsBackToTheASuffixedName()
        {
            var helper = ApiDocsHelper.FromEntries(new Dictionary<string, ApiDetails>(StringComparer.Ordinal)
            {
                ["SomeApiA"] = new() { Description = "An ANSI-only api." },
            });

            var actual = helper.GetXmlDocsForFunction("SomeApi");

            Assert.AreEqual("/// <summary>An ANSI-only api.</summary>", actual);
        }

        [TestMethod]
        public void ReturnsNullWhenNoEntryExistsAtAll()
        {
            var helper = ApiDocsHelper.FromEntries(new Dictionary<string, ApiDetails>(StringComparer.Ordinal));

            var actual = helper.GetXmlDocsForFunction("NotDocumented");

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void RepeatedLookupsForTheSameNameReturnTheSameRenderedText()
        {
            var helper = ApiDocsHelper.FromEntries(new Dictionary<string, ApiDetails>(StringComparer.Ordinal)
            {
                ["GetCursorPos"] = new() { Description = "Retrieves the cursor's position." },
            });

            var first = helper.GetXmlDocsForFunction("GetCursorPos");
            var second = helper.GetXmlDocsForFunction("GetCursorPos");

            Assert.AreEqual(first, second);
        }
    }

    [TestClass]
    public sealed class MergeFromStreamTests
    {
        [TestMethod]
        public void EntriesFromTheStreamAreMergedIn()
        {
            var merged = new Dictionary<string, ApiDetails>(StringComparer.Ordinal);
            using var stream = ApiDocsHelperTests.MergeFromStreamTests.Serialize(new Dictionary<string, ApiDetails>(StringComparer.Ordinal)
            {
                ["GetCursorPos"] = new() { Description = "Retrieves the cursor's position." },
            });

            ApiDocsHelper.MergeFromStream(stream, merged);

            Assert.AreEqual("Retrieves the cursor's position.", merged["GetCursorPos"].Description);
        }

        [TestMethod]
        public void FirstStreamWinsOnAConflictingKey()
        {
            var merged = new Dictionary<string, ApiDetails>(StringComparer.Ordinal);
            using var first = ApiDocsHelperTests.MergeFromStreamTests.Serialize(new Dictionary<string, ApiDetails>(StringComparer.Ordinal)
            {
                ["GetCursorPos"] = new() { Description = "First description." },
            });
            using var second = ApiDocsHelperTests.MergeFromStreamTests.Serialize(new Dictionary<string, ApiDetails>(StringComparer.Ordinal)
            {
                ["GetCursorPos"] = new() { Description = "Second description." },
            });

            ApiDocsHelper.MergeFromStream(first, merged);
            ApiDocsHelper.MergeFromStream(second, merged);

            Assert.AreEqual("First description.", merged["GetCursorPos"].Description);
        }

        // ApiDetailsFormatter.Serialize deliberately throws (this generator
        // only ever reads a real win32docs file, never writes one), so the
        // fixed-position array shape it expects on read has to be written
        // by hand here instead of round-tripping through it.
        private static MemoryStream Serialize(Dictionary<string, ApiDetails> data)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteMapHeader(data.Count);
            foreach (var pair in data)
            {
                writer.Write(pair.Key);
                writer.WriteArrayHeader(6);
                ApiDocsHelperTests.MergeFromStreamTests.WriteStringOrNil(ref writer, pair.Value.HelpLink?.ToString());
                ApiDocsHelperTests.MergeFromStreamTests.WriteStringOrNil(ref writer, pair.Value.Description);
                ApiDocsHelperTests.MergeFromStreamTests.WriteStringOrNil(ref writer, pair.Value.Remarks);
                MessagePackSerializer.Serialize(ref writer, pair.Value.Parameters, MessagePackSerializerOptions.Standard);
                MessagePackSerializer.Serialize(ref writer, pair.Value.Fields, MessagePackSerializerOptions.Standard);
                ApiDocsHelperTests.MergeFromStreamTests.WriteStringOrNil(ref writer, pair.Value.ReturnValue);
            }

            writer.Flush();
            return new MemoryStream(bufferWriter.WrittenSpan.ToArray());
        }

        private static void WriteStringOrNil(ref MessagePackWriter writer, string? value)
        {
            if (value is null)
            {
                writer.WriteNil();
            }
            else
            {
                writer.Write(value);
            }
        }
    }
}
