using MessagePack;
using MessagePack.Formatters;

namespace FancyMouse.Win32Gen.ApiDocs;

/// <summary>
/// Hand-written MessagePack formatter for <see cref="ApiDetails"/>, copied
/// from CsWin32's own private formatter (see
/// https://github.com/microsoft/CsWin32/blob/main/src/Microsoft.Windows.CsWin32/Docs.cs).
/// </summary>
/// <remarks>
/// Required because each entry in the win32docs file is encoded as a
/// fixed-position array (index 0 = help link, 1 = description, 2 =
/// remarks, 3 = parameters, 4 = fields, 5 = return value), not a map with
/// named keys - MessagePack's automatic object resolvers can't read that
/// shape without an explicit formatter telling them what each position
/// means.
/// </remarks>
internal sealed class ApiDetailsFormatter : IMessagePackFormatter<ApiDetails>
{
    public ApiDetails Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        string? helpLink = null;
        string? description = null;
        string? remarks = null;
        Dictionary<string, string>? parameters = null;
        Dictionary<string, string>? fields = null;
        string? returnValue = null;

        var count = reader.ReadArrayHeader();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    helpLink = reader.ReadString();
                    break;
                case 1:
                    description = reader.ReadString();
                    break;
                case 2:
                    remarks = reader.ReadString();
                    break;
                case 3:
                    parameters = options.Resolver.GetFormatterWithVerify<Dictionary<string, string>>().Deserialize(ref reader, options);
                    break;
                case 4:
                    fields = options.Resolver.GetFormatterWithVerify<Dictionary<string, string>>().Deserialize(ref reader, options);
                    break;
                case 5:
                    returnValue = reader.ReadString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new ApiDetails
        {
            HelpLink = helpLink is null ? null : new Uri(helpLink),
            Description = description,
            Remarks = remarks,
            Parameters = parameters ?? new Dictionary<string, string>(StringComparer.Ordinal),
            Fields = fields ?? new Dictionary<string, string>(StringComparer.Ordinal),
            ReturnValue = returnValue,
        };
    }

    public void Serialize(ref MessagePackWriter writer, ApiDetails value, MessagePackSerializerOptions options)
        => throw new NotSupportedException("Writing isn't needed - this generator only ever reads the win32docs file CsWin32 itself ships.");
}
