using FancyMouse.Models.Display;

namespace FancyMouse.WinUI3.Controls.Editor;

/// <summary>
/// A screen's placement in <see cref="ScreenLayoutEditor"/> - its position and size in desktop
/// pixel coordinates (the same space as <see cref="ScreenInfo.DisplayArea"/>), starting at its
/// Display Settings position at its native size.
/// </summary>
/// <remarks>
/// The size can only change by a uniform <see cref="Scale"/> - a screen's aspect ratio is fixed
/// by its resolution, so stretching one axis would only distort its contents. Deliberately has no
/// dependency on XAML, so the placement logic can move into a shared library later.
/// </remarks>
public sealed class EditableScreen
{
    public EditableScreen(ScreenInfo screenInfo, int number)
    {
        this.ScreenInfo = screenInfo ?? throw new ArgumentNullException(nameof(screenInfo));
        this.Number = number;
        this.ResetToSystemPosition();
    }

    public ScreenInfo ScreenInfo
    {
        get;
    }

    /// <summary>
    /// Gets the number shown on the screen in the editor - 1-based, in the order the screens
    /// were loaded (i.e. Display Settings' own enumeration order).
    /// </summary>
    public int Number
    {
        get;
    }

    public double X
    {
        get;
        set;
    }

    public double Y
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the screen's size relative to its native resolution (1 = native size).
    /// </summary>
    public double Scale
    {
        get;
        set;
    }

    public double NativeWidth => (double)this.ScreenInfo.DisplayArea.Width;

    public double NativeHeight => (double)this.ScreenInfo.DisplayArea.Height;

    public double Width => this.NativeWidth * this.Scale;

    public double Height => this.NativeHeight * this.Scale;

    /// <summary>
    /// Gets a value indicating whether the screen has been moved or resized away from its
    /// Display Settings position.
    /// </summary>
    public bool IsCustomised =>
        (this.X != (double)this.ScreenInfo.DisplayArea.X)
        || (this.Y != (double)this.ScreenInfo.DisplayArea.Y)
        || (this.Scale != 1);

    /// <summary>
    /// Forgets any customisation and puts the screen back where Display Settings has it, at its
    /// native size.
    /// </summary>
    public void ResetToSystemPosition()
    {
        this.X = (double)this.ScreenInfo.DisplayArea.X;
        this.Y = (double)this.ScreenInfo.DisplayArea.Y;
        this.Scale = 1;
    }
}
