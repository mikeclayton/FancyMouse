using System.Drawing;
using System.Text.Json;
using FancyMouse.Common.Models.Display;

namespace FancyMouse.Common.Helpers;

public sealed class MwbClient
{
    public const int LocalApiServerPort = 15102;
    public const int RemoteApiServerPort = 15103;

    public MwbClient(string machineKey)
    {
        this.MachineKey = machineKey ?? throw new ArgumentNullException(nameof(machineKey));
    }

    private HttpClient HttpClient => new()
    {
        BaseAddress = new Uri($"http://localhost:{LocalApiServerPort}"),
        Timeout = TimeSpan.FromMilliseconds(2500),
    };

    public string MachineKey
    {
        get;
    }

    public async Task<string[]> GetMachineMatrix()
    {
        var requestUrl = $"/v1/matrix?key={this.MachineKey}";
        var responseJson = await this.HttpClient.GetStringAsync(requestUrl);
        var responseMatrix = JsonSerializer.Deserialize<string[]>(responseJson)
            ?? throw new InvalidOperationException();
        return responseMatrix;
    }

    public async Task<ScreenInfo[]> GetMachineScreens(string machineId)
    {
        MwbClient.ValidateMachineId(machineId);
        var requestUrl = $"/v1/machines/{machineId}/screens?key={this.MachineKey}";
        var responseJson = await this.HttpClient.GetStringAsync(requestUrl);
        var responseScreens = JsonSerializer.Deserialize<ScreenInfo[]>(responseJson)
            ?? throw new InvalidOperationException();
        return responseScreens;
    }

    public async Task<Image?> GetScreenshot(string machineId, int screenId, int width, int height)
    {
        try
        {
            MwbClient.ValidateMachineId(machineId);
            var requestUrl = $"/v1/machines/{machineId}/screens/{screenId}/screenshot?{width}&{height}?key={this.MachineKey}";
            using var responseStream = await this.HttpClient.GetStreamAsync(requestUrl);
            var responseImage = Bitmap.FromStream(responseStream);
            return responseImage;
        }
        catch
        {
            return null;
        }
    }

    private static void ValidateMachineId(string machineId)
    {
        const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
        ArgumentNullException.ThrowIfNullOrWhiteSpace(machineId);
        machineId = machineId.Trim();
        if (string.IsNullOrEmpty(machineId))
        {
            throw new ArgumentException("Machine ID cannot be empty.", nameof(machineId));
        }

        foreach (char c in machineId)
        {
            if (!allowedChars.Contains(c))
            {
                throw new ArgumentException($"Machine ID contains an invalid character. Allowed characters are: {allowedChars}", nameof(machineId));
            }
        }
    }
}
