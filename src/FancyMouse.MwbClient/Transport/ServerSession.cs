using System.Net.Sockets;
using System.Threading.Channels;

using FancyMouse.MwbClient.Models;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1848 // Use the LoggerMethod delegate
namespace FancyMouse.MwbClient.Transport;

public sealed class ServerSession : IDisposable
{
    public ServerSession(ILogger logger, ServerEndpoint server, TcpClient tcpClient)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.Server = server ?? throw new ArgumentNullException(nameof(server));
        this.TcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        this.Outbox = Channel.CreateBounded<Message>(250);
        this.StopTokenSource = new CancellationTokenSource();
    }

    public ILogger Logger
    {
        get;
    }

    private ServerEndpoint Server
    {
        get;
    }

    public TcpClient TcpClient
    {
        get;
    }

    /// <summary>
    /// Gets the channel used to buffer messages internally while they wait to be sent to the server.
    /// </summary>
    private Channel<Message> Outbox
    {
        get;
    }

    private CancellationTokenSource StopTokenSource
    {
        get;
    }

    private bool Disposed
    {
        get;
        set;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        // await a dummy task to the compiler doesn't complain about "async"
        await Task.CompletedTask.ConfigureAwait(false);

        // create a combined cancellation token so the caller can stop the client,
        // or we can stop it without having to cancel the caller's token
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.StopTokenSource.Token, cancellationToken);

        // listen for messages coming up from the client
        // (start this first so we don't miss any incoming messages)
        _ = Task.Run(() => this.ReceiveMessagesAsync(linkedCts.Token), cancellationToken);

        // pump messages from the session's "send" buffer down to the client
        this.Logger.LogInformation("session: starting network writer");
        _ = Task.Run(() => EndpointHelper.StartNetworkSenderAsync(this.Outbox, this.TcpClient, linkedCts.Token), cancellationToken);
        this.Logger.LogInformation("session: network writer started...");
    }

    /// <summary>
    /// Puts a message in the session's outbox buffer to be sent to the client.
    /// </summary>
    public async Task SendMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        await this.Outbox.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);
    }

    internal async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var inboundStream = this.TcpClient.GetStream();
        while (!cancellationToken.IsCancellationRequested)
        {
            // read the next incoming message
            var message = await EndpointHelper.ReadMessageAsync(inboundStream, cancellationToken).ConfigureAwait(false);
            if (message == null)
            {
                return;
            }

            // process the message
            await this.Server.ReceiveMessageAsync(this, message, cancellationToken).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (this.Disposed)
        {
            return;
        }

        if (disposing)
        {
            this.TcpClient?.Dispose();
        }

        this.Disposed = true;
    }
}
