using SkiaSharp;

namespace FancyMouse.Drawing.Blurring;

/// <summary>
/// Represents the state of the blurred screenshot images for one physical screen
/// within a <see cref="BlurPipeline"/> - see its remarks for what
/// "todo" / "doing" (<see cref="BlurInProgress"/>) / "done" mean.
/// </summary>
internal sealed class BlurState
{
    public bool BlurInProgress
    {
        get;
        set;
    }

    public SKBitmap? Todo
    {
        get;
        set;
    }

    public SKBitmap? Done
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
