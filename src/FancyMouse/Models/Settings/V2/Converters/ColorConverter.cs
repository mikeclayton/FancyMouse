#nullable enable

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FancyMouse.Models.Settings.V2;

/// <summary>
/// Converts a color string from the settings file into a color, and vice versa.
/// </summary>
/// <remarks>
/// "Color.Red"              => Color.Red
/// "SystemColors.Highlight" => SystemColors.Highlight
/// "#AABBCC"                => Color.FromArgb(0xFF, 0xAA, 0xBB, 0xCC)
/// </remarks>
internal sealed class ColorConverter : JsonConverter<Color?>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(string);
    }

    public override Color? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return ColorConverter.Deserialize(value);
    }

    public override void Write(Utf8JsonWriter writer, Color? value, JsonSerializerOptions options)
    {
        var str = ColorConverter.Serialize(value);
        if (str is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(str);
        }
    }

    internal static string? Serialize(Color? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var color = value.Value;
        return color switch
        {
            Color { IsNamedColor: true } =>
                $"{nameof(Color)}.{color.Name}",
            Color { IsSystemColor: true } =>
                $"{nameof(SystemColors)}.{color.Name}",
            _ =>
                $"#{color.R:X2}{color.G:X2}{color.B:X2}",
        };
    }

    internal static Color? Deserialize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // e.g. "#AABBCC"
        if (value.StartsWith('#'))
        {
            var culture = CultureInfo.InvariantCulture;
            if ((value.Length == 7)
                && int.TryParse(value[1..3], NumberStyles.HexNumber, culture, out var r)
                && int.TryParse(value[3..5], NumberStyles.HexNumber, culture, out var g)
                && int.TryParse(value[5..7], NumberStyles.HexNumber, culture, out var b))
            {
                return Color.FromArgb(0xFF, r, g, b);
            }
        }

        const StringComparison comparison = StringComparison.InvariantCulture;

        // e.g. "Color.Red"
        const string colorPrefix = $"{nameof(Color)}.";
        if (value.StartsWith(colorPrefix, comparison))
        {
            var colorName = value[colorPrefix.Length..];
            var property = typeof(Color).GetProperties()
                .SingleOrDefault(property => property.Name == colorName);
            if (property is not null)
            {
                return (Color?)property.GetValue(null, null);
            }
        }

        // e.g. "SystemColors.Highlight"
        const string systemColorPrefix = $"{nameof(SystemColors)}.";
        if (value.StartsWith(systemColorPrefix, comparison))
        {
            var colorName = value[systemColorPrefix.Length..];
            var property = typeof(SystemColors).GetProperties()
                .SingleOrDefault(property => property.Name == colorName);
            if (property is not null)
            {
                return (Color?)property.GetValue(null, null);
            }
        }

        return null;
    }
}
