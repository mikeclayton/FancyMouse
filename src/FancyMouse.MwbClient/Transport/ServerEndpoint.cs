using System.Net;
using System.Net.Sockets;

using FancyMouse.MwbClient.Models;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1848 // Use the LoggerMethod delegate
namespace FancyMouse.MwbClient.Transport;

public sealed class ServerEndpoint : IDisposable
{
    public delegate Task MessageReceivedCallback(ServerEndpoint server, ServerSession session, Message message, CancellationToken cancellationToken);

    public ServerEndpoint(ILogger logger, string name, IPAddress address, int port, MessageReceivedCallback callback)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
        this.Address = address ?? throw new ArgumentNullException(nameof(address));
        this.Port = port;
        this.Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    ~ServerEndpoint()
    {
        this.Dispose(false);
    }

    private ILogger Logger
    {
        get;
    }

    /// <summary>
    /// Gets a name for the server endpoint that can be used to identify
    /// it in log messages and other diagnostic output.
    /// </summary>
    public string Name
    {
        get;
    }

    public IPAddress Address
    {
        get;
    }

    public int Port
    {
        get;
    }

    public MessageReceivedCallback Callback
    {
        get;
    }

    private TcpClient? TcpClient
    {
        get;
        set;
    }

    private bool Disposed
    {
        get;
        set;
    }

    public async Task StartServerAsync(CancellationToken cancellationToken = default)
    {
        this.Logger.LogInformation("server: starting listener...");
        var listener = new TcpListener(this.Address, this.Port);
        listener.Start();
        this.Logger.LogInformation("server: listener started...");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            var clientEndpoint = client.Client.RemoteEndPoint as IPEndPoint
                ?? throw new InvalidOperationException();
            this.Logger.LogInformation(
                "server: client connection accepted from '{RemoteEndpointAddress}:{RemoteEndpointPort}'",
                clientEndpoint.Address,
                clientEndpoint.Port);

            _ = Task.Run(() => this.HandleClientAsync(client, cancellationToken), cancellationToken);
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken = default)
    {
        this.Logger.LogInformation($"server: {nameof(HandleClientAsync)}");
        var serverSession = new ServerSession(this.Logger, this, tcpClient);
        await serverSession.ConnectAsync(cancellationToken).ConfigureAwait(false);
    }

    internal async Task ReceiveMessageAsync(ServerSession session, Message message, CancellationToken cancellationToken)
    {
        await this.Callback.Invoke(this, session, message, cancellationToken).ConfigureAwait(false);
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
