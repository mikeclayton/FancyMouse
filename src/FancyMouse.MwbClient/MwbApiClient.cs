using System.Drawing;
using System.Net;
using System.Text;
using System.Text.Json;

using FancyMouse.Common.Models.Display;
using FancyMouse.MwbClient.Models;
using FancyMouse.MwbClient.Transport;
using Microsoft.Extensions.Logging;

namespace FancyMouse.Common.Helpers;

public sealed class MwbApiClient : IDisposable
{
    public MwbApiClient(ILogger logger, string name, IPAddress serverAddress, int serverPort)
    {
        this.ClientEndpoint = new ClientEndpoint(
            logger: logger ?? throw new ArgumentNullException(nameof(logger)),
            name: name ?? throw new ArgumentNullException(nameof(name)),
            serverAddress: serverAddress ?? throw new ArgumentNullException(nameof(serverAddress)),
            serverPort: serverPort);
    }

    ~MwbApiClient()
    {
        this.Dispose(false);
    }

    private ClientEndpoint ClientEndpoint
    {
        get;
    }

    private bool Disposed
    {
        get;
        set;
    }

    public async Task<MachineMatrixResponse> GetMachineMatrix(CancellationToken cancellationToken = default)
    {
        await this.ClientEndpoint.ConnectAsync(cancellationToken).ConfigureAwait(false);

        // send the machine matrix request
        var machineMatrixRequest = Message.ToMessage(
            correlationId: 1,
            messageType: (int)MessageType.MachineMatrixRequest);
        await this.ClientEndpoint.SendMessageAsync(machineMatrixRequest, cancellationToken).ConfigureAwait(false);

        // wait for the machine matrix response
        var machineMatrixResponse = await this.ClientEndpoint.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
        var machineMatrixJson = Encoding.UTF8.GetString(machineMatrixResponse.MessageData ?? throw new InvalidOperationException());
        var machineMatrix = JsonSerializer.Deserialize<MachineMatrixResponse>(machineMatrixJson);

        return machineMatrix;
    }

    public async Task<List<ScreenInfo>> GetRemoteScreenInfo(CancellationToken cancellationToken = default)
    {
        await this.ClientEndpoint.ConnectAsync(cancellationToken).ConfigureAwait(false);

        // send the screen info request
        var screenInfoRequest = Message.ToMessage(
            correlationId: 2,
            messageType: (int)MessageType.ScreenInfoRequest);
        await this.ClientEndpoint.SendMessageAsync(screenInfoRequest, cancellationToken).ConfigureAwait(false);

        // wait for the screen info response
        var screenInfoResponse = await this.ClientEndpoint.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
        var screenInfoJson = Encoding.UTF8.GetString(screenInfoResponse.MessageData ?? throw new InvalidOperationException());
        var screenInfo = JsonSerializer.Deserialize<ScreenInfoResponse>(screenInfoJson);

        return screenInfo.ScreenInfo;
    }

    public async Task<Bitmap> GetRemoteThumbnail(int screenId, int sourceX, int sourceY, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, CancellationToken cancellationToken = default)
    {
        await this.ClientEndpoint.ConnectAsync(cancellationToken).ConfigureAwait(false);

        // send the thumbnail request
        var thumbnailRequest = Message.ToMessage(
            correlationId: 3,
            messageType: (int)MessageType.ThumbnailRequest,
            payload: new ThumbnailRequest
            {
                ScreenId = screenId,
                SourceX = sourceX,
                SourceY = sourceY,
                SourceWidth = sourceWidth,
                SourceHeight = sourceHeight,
                TargetWidth = targetWidth,
                TargetHeight = targetHeight,
            });
        await this.ClientEndpoint.SendMessageAsync(thumbnailRequest, cancellationToken).ConfigureAwait(false);

        // wait for the thumbnail response
        var thumbnailResponse = await this.ClientEndpoint.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
        using var thumbnailStream = new MemoryStream(thumbnailResponse.MessageData ?? throw new InvalidOperationException());
        var thumbnailBitmap = new Bitmap(thumbnailStream);
        return thumbnailBitmap;
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
            this.ClientEndpoint.Dispose();
        }

        this.Disposed = true;
    }
}
