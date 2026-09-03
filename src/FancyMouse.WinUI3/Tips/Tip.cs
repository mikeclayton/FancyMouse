namespace FancyMouse.WinUI3.Tips;

/// <summary>
/// Base view model for a <see cref="TipBar"/> item
/// </summary>
public abstract class Tip
{
    protected Tip(string? icon)
    {
        this.Icon = icon;
    }

    public string? Icon
    {
        get;
    }
}
