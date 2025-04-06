using System.Net;
using FancyMouse.Common.Helpers;
using FancyMouse.MwbClient.Models;
using FancyMouse.MwbClient.Transport;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1848 // Use the LoggerMethod delegate
namespace FancyMouse.MwbClient;

public static class MwbApiServer
{
    public static async Task RunAsync(CancellationToken cancellationToken)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
        });
        var logger = loggerFactory.CreateLogger<Message>();

        logger.LogInformation("Hello, World!");

        var server = new ServerEndpoint(
            logger: logger,
            name: "server",
            address: IPAddress.Any,
            port: 12345,
            callback: MwbApiServer.ReceiveMessageAsync);

        var task = Task.Run(
            () => server.StartServerAsync(cancellationToken).ConfigureAwait(false));

        try
        {
            // wait for the task to be cancelled
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // swallow this exception
        }

        logger.LogInformation("Goodbye, World!");
    }

    private static async Task ReceiveMessageAsync(ServerEndpoint server, ServerSession session, Message message, CancellationToken cancellationToken)
    {
        // process the message
        switch ((MessageType)message.MessageType)
        {
            case MessageType.MachineMatrixRequest:
                /*
                var machineMatrix = MachineStuff.MachineMatrix
                    .Where(machine => !string.IsNullOrEmpty(machine))
                    .Select(machine => machine.Trim())
                    .ToList();
                */
                var machineMatrix = new List<string> { "aaa", "bbb" };
                var machineMatrixResponse = Message.ToMessage(
                    correlationId: message.CorrelationId,
                    messageType: (int)MessageType.MachineMatrixResponse,
                    payload: new MachineMatrixResponse(machineMatrix));
                await session.SendMessageAsync(machineMatrixResponse, cancellationToken).ConfigureAwait(false);
                break;
            case MessageType.ScreenInfoRequest:
                var screenInfo = ScreenHelper.GetAllScreens().ToList();
                var screenInfoResponse = Message.ToMessage(
                    correlationId: message.CorrelationId,
                    messageType: (int)MessageType.ScreenInfoResponse,
                    payload: new ScreenInfoResponse(screenInfo));
                await session.SendMessageAsync(screenInfoResponse, cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
