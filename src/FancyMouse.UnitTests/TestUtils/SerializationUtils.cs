using System.Text.Json;
using System.Text.Json.Serialization;

namespace FancyMouse.UnitTests.TestUtils;

internal static class SerializationUtils
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string SerializeAnonymousType<T>(T value)
    {
        return JsonSerializer.Serialize(value, SerializationUtils.JsonSerializerOptions);
    }
}
