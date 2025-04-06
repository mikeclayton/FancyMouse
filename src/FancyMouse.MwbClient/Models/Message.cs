using System.Text;
using System.Text.Json;

namespace FancyMouse.MwbClient.Models;

public sealed class Message
{
    public Message(int correlationId, int messageType)
        : this(correlationId, messageType, null)
    {
    }

    public Message(int correlationId, int messageType, byte[]? messageData)
    {
        this.CorrelationId = correlationId;
        this.MessageType = messageType;
        this.MessageData = messageData;
    }

    public int CorrelationId
    {
        get;
    }

    public int MessageType
    {
        get;
    }

    public byte[]? MessageData
    {
        get;
    }

    public T? ToObject<T>()
    {
        if (this.MessageData == null)
        {
            throw new InvalidOperationException();
        }

        var result = JsonSerializer.Deserialize<T>(this.MessageData);
        return result;
    }

    public static Message ToMessage(int correlationId, int messageType)
    {
        return new Message(correlationId, messageType);
    }

    public static Message ToMessage<T>(int correlationId, int messageType, T payload)
        where T : struct
    {
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        return new Message(correlationId, messageType, bytes);
    }

    public static byte[] Serialize<T>(T payload)
        where T : struct
    {
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        return bytes;
    }
}
