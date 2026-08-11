using System.Buffers;

using FancyMouse.Win32Gen.ApiDocs;

using MessagePack;

namespace FancyMouse.Win32Gen.UnitTests.ApiDocs;

public static class ApiDetailsFormatterTests
{
    [TestClass]
    public sealed class DeserializeTests
    {
        [TestMethod]
        public void AllSixPositionsAreReadInOrder()
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteArrayHeader(6);
            writer.Write("https://example.com/help");
            writer.Write("description text");
            writer.Write("remarks text");
            MessagePackSerializer.Serialize(ref writer, new Dictionary<string, string>(StringComparer.Ordinal) { ["hWnd"] = "handle desc" });
            MessagePackSerializer.Serialize(ref writer, new Dictionary<string, string>(StringComparer.Ordinal) { ["Field1"] = "field desc" });
            writer.Write("return value text");
            writer.Flush();

            var reader = new MessagePackReader(bufferWriter.WrittenMemory);
            var details = new ApiDetailsFormatter().Deserialize(ref reader, MessagePackSerializerOptions.Standard);

            Assert.AreEqual(new Uri("https://example.com/help"), details.HelpLink);
            Assert.AreEqual("description text", details.Description);
            Assert.AreEqual("remarks text", details.Remarks);
            Assert.AreEqual("handle desc", details.Parameters["hWnd"]);
            Assert.AreEqual("field desc", details.Fields["Field1"]);
            Assert.AreEqual("return value text", details.ReturnValue);
        }

        [TestMethod]
        public void NilPositionsBecomeNullOrEmpty()
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteArrayHeader(6);
            writer.WriteNil();
            writer.Write("description text");
            writer.WriteNil();
            MessagePackSerializer.Serialize(ref writer, new Dictionary<string, string>(StringComparer.Ordinal));
            MessagePackSerializer.Serialize(ref writer, new Dictionary<string, string>(StringComparer.Ordinal));
            writer.WriteNil();
            writer.Flush();

            var reader = new MessagePackReader(bufferWriter.WrittenMemory);
            var details = new ApiDetailsFormatter().Deserialize(ref reader, MessagePackSerializerOptions.Standard);

            Assert.IsNull(details.HelpLink);
            Assert.AreEqual("description text", details.Description);
            Assert.IsNull(details.Remarks);
            Assert.AreEqual(0, details.Parameters.Count);
            Assert.AreEqual(0, details.Fields.Count);
            Assert.IsNull(details.ReturnValue);
        }

        [TestMethod]
        public void ShorterArrayLeavesTrailingFieldsAtDefaults()
        {
            // real win32docs entries sometimes truncate the array rather
            // than nil-filling every trailing position - Deserialize must
            // tolerate that, not just a fixed 6-element array.
            var bufferWriter = new ArrayBufferWriter<byte>();
            var writer = new MessagePackWriter(bufferWriter);
            writer.WriteArrayHeader(2);
            writer.WriteNil();
            writer.Write("description only");
            writer.Flush();

            var reader = new MessagePackReader(bufferWriter.WrittenMemory);
            var details = new ApiDetailsFormatter().Deserialize(ref reader, MessagePackSerializerOptions.Standard);

            Assert.IsNull(details.HelpLink);
            Assert.AreEqual("description only", details.Description);
            Assert.IsNull(details.Remarks);
            Assert.AreEqual(0, details.Parameters.Count);
            Assert.IsNull(details.ReturnValue);
        }
    }
}
