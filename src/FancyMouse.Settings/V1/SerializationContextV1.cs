using System.Text.Json.Serialization;

namespace FancyMouse.Settings.V1;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppConfig))]
internal sealed partial class SerializationContextV1 : JsonSerializerContext
{
}
