using System.Text.Json;

namespace FancyMouse.UnitTests.TestUtils;

internal static class SerializationUtils
{
    public static string SerializeAnonymousType<T>(T value)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        return JsonSerializer.Serialize(value, options);
    }
}
