using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

using FancyMouse.MwbClient.Models;

namespace FancyMouse.MwbClient.Transport;

internal static class EndpointHelper
{
    /// <summary>
    /// Starts the network sender.
    /// Reads messages from the "sender" channel and writes them to the network stream.
    /// </summary>
    public static async Task StartNetworkSenderAsync(Channel<Message> sendBuffer, TcpClient outboundClient, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(outboundClient);
        if (!outboundClient.Connected)
        {
            // server disconnected
            throw new InvalidOperationException("tcp client is disconnected");
        }

        var channelReader = sendBuffer.Reader;
        var outboundStream = outboundClient.GetStream();
        while (!cancellationToken.IsCancellationRequested)
        {
            // read a message from the "sender" channel
            var message = await channelReader.ReadAsync(cancellationToken).ConfigureAwait(false);

            // write the message to the network stream for the server to pick up
            await EndpointHelper.WriteMessageAsync(outboundStream, message, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Starts the network receiver.
    /// Reads messages from the network stream and writes them to the "receive" channel.
    /// </summary>
    public static async Task StartNetworkReceiverAsync(TcpClient inboundClient, Channel<Message> receiveBuffer, CancellationToken cancellationToken)
    {
        // create a combined cancellation token so the caller can stop the client,
        // or we can stop it without having to cancel the caller's token
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, cancellationToken);

        var inboundStream = (inboundClient ?? throw new InvalidOperationException()).GetStream();
        var channelWriter = receiveBuffer.Writer;
        while (!linkedCts.IsCancellationRequested)
        {
            // read a message from the network stream
            var message = await EndpointHelper.ReadMessageAsync(inboundStream, linkedCts.Token).ConfigureAwait(false);
            if (message == null)
            {
                return;
            }

            // write the message to the "receive" channel for the caller to pick up
            await channelWriter.WriteAsync(message, linkedCts.Token).ConfigureAwait(false);
        }
    }

    public static async Task<Message?> ReadMessageAsync(Stream inboundStream, CancellationToken cancellationToken)
    {
        // read the correlation id
        var correlationIdBuffer = new byte[4];
        var correlationIdBytesRead = await EndpointHelper.ReadExactlyAsync(inboundStream, correlationIdBuffer, correlationIdBuffer.Length, cancellationToken).ConfigureAwait(false);
        if (correlationIdBytesRead != correlationIdBuffer.Length)
        {
            // client disconnected?
            return null;
        }

        var correlationId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(correlationIdBuffer, 0));

        // read the message type
        var messageTypeBuffer = new byte[4];
        var messageTypeBytesRead = await EndpointHelper.ReadExactlyAsync(inboundStream, messageTypeBuffer, messageTypeBuffer.Length, cancellationToken).ConfigureAwait(false);
        if (messageTypeBytesRead != messageTypeBuffer.Length)
        {
            // client disconnected?
            return null;
        }

        var messageType = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(messageTypeBuffer, 0));

        // read the data length
        var messageDataLengthBuffer = new byte[4];
        var messageDataLengthBytesRead = await EndpointHelper.ReadExactlyAsync(inboundStream, messageDataLengthBuffer, 4, cancellationToken).ConfigureAwait(false);
        if (messageDataLengthBytesRead != messageDataLengthBuffer.Length)
        {
            // client disconnected?
            return null;
        }

        var messageDataLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(messageDataLengthBuffer, 0));

        // read the data buffer
        var messageData = new byte[messageDataLength];
        var messageDataBytesRead = await EndpointHelper.ReadExactlyAsync(inboundStream, messageData, messageDataLength, cancellationToken).ConfigureAwait(false);
        if (messageDataBytesRead != messageData.Length)
        {
            // client disconnected?
            return null;
        }

        var message = new Message(correlationId, messageType, messageData);
        return message;
    }

    private static async Task<int> ReadExactlyAsync(Stream inboundStream, byte[] buffer, int count, CancellationToken cancellationToken)
    {
        int totalBytes = 0;
        while (totalBytes < count)
        {
            var bytesRead = await inboundStream.ReadAsync(
                buffer: buffer.AsMemory(totalBytes, count - totalBytes),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                break;
            }

            totalBytes += bytesRead;
        }

        return totalBytes;
    }

    public static async Task WriteMessageAsync(Stream outboundStream, Message message, CancellationToken token = default)
    {
        await EndpointHelper.WriteMessageAsync(outboundStream, message.CorrelationId, message.MessageType, message.MessageData, token).ConfigureAwait(false);
    }

    public static async Task WriteMessageAsync(Stream outboundStream, int correlationId, int messageType, byte[]? messageData, CancellationToken cancellationToken = default)
    {
        // write the correlation id
        var correlationIdBuffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(correlationId));
        await outboundStream.WriteAsync(correlationIdBuffer, cancellationToken).ConfigureAwait(false);

        // write the message type
        var messageTypeBuffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageType));
        await outboundStream.WriteAsync(messageTypeBuffer, cancellationToken).ConfigureAwait(false);

        // write the data length
        var messageLength = messageData?.Length ?? 0;
        var messageLengthBuffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(messageLength));
        await outboundStream.WriteAsync(messageLengthBuffer, cancellationToken).ConfigureAwait(false);

        // write the data buffer
        if (messageData != null)
        {
            await outboundStream.WriteAsync(messageData, cancellationToken).ConfigureAwait(false);
        }

        await outboundStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
