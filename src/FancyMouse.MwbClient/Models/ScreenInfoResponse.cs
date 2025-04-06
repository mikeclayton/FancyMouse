using System.Text.Json.Serialization;

using FancyMouse.Common.Models.Display;

namespace FancyMouse.MwbClient.Models;

public struct ScreenInfoResponse
{
    [JsonInclude]
    public List<ScreenInfo> ScreenInfo;

    public ScreenInfoResponse(List<ScreenInfo> screenInfo)
    {
        this.ScreenInfo = screenInfo ?? throw new ArgumentNullException(nameof(screenInfo));
    }
}
