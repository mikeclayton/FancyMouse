using System.Text.Json.Serialization;

namespace FancyMouse.MwbClient.Models;

public struct ThumbnailResponse
{
    [JsonInclude]
    public byte[] ImageBytes;

    public ThumbnailResponse(byte[] imageBytes)
    {
        this.ImageBytes = imageBytes ?? throw new ArgumentNullException(nameof(imageBytes));
    }
}
