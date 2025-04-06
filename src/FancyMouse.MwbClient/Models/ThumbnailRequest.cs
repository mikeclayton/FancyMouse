using System.Text.Json.Serialization;

namespace FancyMouse.MwbClient.Models;

public struct ThumbnailRequest
{
    [JsonInclude]
    public int ScreenId;

    [JsonInclude]
    public int SourceX;

    [JsonInclude]
    public int SourceY;

    [JsonInclude]
    public int SourceWidth;

    [JsonInclude]
    public int SourceHeight;

    [JsonInclude]
    public int TargetWidth;

    [JsonInclude]
    public int TargetHeight;
}
