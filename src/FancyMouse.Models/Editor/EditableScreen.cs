using FancyMouse.Models.Display;

namespace FancyMouse.Models.Editor;

/// <summary>
/// A screen's placement in a <see cref="PreviewDesign"/> - the position and size of its outer
/// box (margin included) in preview pixels, relative to the preview's top-left corner. That's the
/// same space as <see cref="Layout.ScreenLayout.ScreenBounds"/>, so a placement means exactly
/// where the screen is drawn when the preview is shown.
/// </summary>
/// <remarks>
/// The size can only change by a uniform <see cref="Scale"/> - LayoutHelper scales a screen's
/// outer box in proportion to its resolution, so the editor keeps the same aspect ratio.
/// </remarks>
public sealed class EditableScreen
{
    public EditableScreen(ScreenInfo screenInfo, int number, double systemX, double systemY, double systemScale)
    {
        this.ScreenInfo = screenInfo ?? throw new ArgumentNullException(nameof(screenInfo));
        this.Number = number;
        this.SystemX = systemX;
        this.SystemY = systemY;
        this.SystemScale = systemScale;
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
    /// Gets or sets the screen's size in preview pixels per desktop pixel - e.g. 0.25 draws a
    /// 1920 x 1080 screen as 480 x 270 in the preview.
    /// </summary>
    public double Scale
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the placement the automatic layout gives this screen - the whole desktop scaled to
    /// fit the preview's content area, the same way LayoutHelper arranges it.
    /// </summary>
    public double SystemX
    {
        get;
    }

    public double SystemY
    {
        get;
    }

    public double SystemScale
    {
        get;
    }

    public double NativeWidth => (double)this.ScreenInfo.DisplayArea.Width;

    public double NativeHeight => (double)this.ScreenInfo.DisplayArea.Height;

    public double Width => this.NativeWidth * this.Scale;

    public double Height => this.NativeHeight * this.Scale;

    /// <summary>
    /// Gets a value indicating whether the screen has been moved or resized away from its
    /// automatic layout position.
    /// </summary>
    public bool IsCustomised =>
        (this.X != this.SystemX)
        || (this.Y != this.SystemY)
        || (this.Scale != this.SystemScale);

    /// <summary>
    /// Forgets any customisation and puts the screen back where the automatic layout has it.
    /// </summary>
    public void ResetToSystemPosition()
    {
        this.X = this.SystemX;
        this.Y = this.SystemY;
        this.Scale = this.SystemScale;
    }
}
