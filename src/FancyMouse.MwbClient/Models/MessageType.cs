namespace FancyMouse.MwbClient.Models;

public enum MessageType
{
    HeartbeatMessage,

    PingRequest,
    PingResponse,

    MachineMatrixRequest,
    MachineMatrixResponse,

    ScreenInfoRequest,
    ScreenInfoResponse,

    ThumbnailRequest,
    ThumbnailResponse,

    ScreenshotStartResponse,
    ScreenshotDataResponse,
    ScreenshotFinishResponse,
}
