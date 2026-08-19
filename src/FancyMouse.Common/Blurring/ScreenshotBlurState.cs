using System.Drawing;

namespace FancyMouse.Common.Blurring;

/// <summary>
/// Represents the state of the blurred screenshot images for one physical screen
/// within a <see cref="ScreenshotBlurPipeline"/> - see its remarks for what
/// "todo" / "doing" (<see cref="BlurInProgress"/>) / "done" mean.
/// </summary>
internal sealed class ScreenshotBlurState
{
    public bool BlurInProgress
    {
        get;
        set;
    }

    public Bitmap? Todo
    {
        get;
        set;
    }

    public Bitmap? Done
    {
        get;
        set;
    }

    public DateTimeOffset DoneAt
    {
        get;
        set;
    }
}
