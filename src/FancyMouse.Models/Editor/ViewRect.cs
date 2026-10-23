namespace FancyMouse.Models.Editor;

/// <summary>
/// A rectangle in a view's own units (e.g. DIPs in a WinUI view) - what <see cref="EditorCamera"/>
/// maps preview pixels to.
/// </summary>
public readonly record struct ViewRect(double X, double Y, double Width, double Height)
{
    public double Right => this.X + this.Width;

    public double Bottom => this.Y + this.Height;
}
