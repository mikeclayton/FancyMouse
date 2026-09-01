namespace FancyMouse.WinUI3.Tips;

/// <summary>
/// A plain, static piece of copy - e.g. a usage tip unrelated to any tracked metric.
/// </summary>
public sealed class InfoTip : Tip
{
    public InfoTip(string message, string? icon = null)
        : base(icon)
    {
        this.Message = message;
    }

    public string Message
    {
        get;
    }
}
