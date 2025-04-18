using System.Text.Json.Serialization;

namespace FancyMouse.Settings.V2;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AppConfig))]
public sealed partial class SerializationContextV2 : JsonSerializerContext
{
}
