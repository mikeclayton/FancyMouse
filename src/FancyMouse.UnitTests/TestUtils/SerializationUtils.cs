using System.Text.Json;

namespace FancyMouse.UnitTests.TestUtils;

internal static class SerializationUtils
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        WriteIndented = true,
    };

    public static string SerializeAnonymousType<T>(T value)
    {
        return JsonSerializer.Serialize(value, SerializationUtils.Options);
    }
}
