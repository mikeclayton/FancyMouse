using System.Drawing;

namespace FancyMouse.Common.Blurring;

/// <summary>
/// One physical screen's blur state within <see cref="ScreenshotBlurPipeline"/> - see its remarks for what
/// "todo"/"doing" (<see cref="BlurInProgress"/>)/"done" mean. Internal - <see cref="ScreenshotBlurPipeline"/>
/// is the only thing that ever touches an instance of this, always under its own lock.
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

    public DateTime DoneAt
    {
        get;
        set;
    }
}
