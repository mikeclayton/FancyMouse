using FancyMouse.Models.Editor;

using Microsoft.UI.Xaml.Controls;

using Image = Microsoft.UI.Xaml.Controls.Image;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace FancyMouse.WinUI3.Controls.Editor;

/// <summary>
/// The UI elements <see cref="ScreenLayoutEditor"/> maintains for one screen - a rounded bezel
/// ring, its background inside it, and the screen's number and resolution, all in one container
/// so the whole screen moves, resizes and restacks as a unit.
/// </summary>
internal sealed class ScreenVisual
{
    public ScreenVisual(EditableScreen screen, CursorGrid container, Image bezelImage, Rectangle fill)
    {
        this.Screen = screen;
        this.Container = container;
        this.BezelImage = bezelImage;
        this.Fill = fill;
    }

    public EditableScreen Screen
    {
        get;
    }

    public CursorGrid Container
    {
        get;
    }

    public Image BezelImage
    {
        get;
    }

    public Rectangle Fill
    {
        get;
    }

    /// <summary>
    /// Gets or sets the size, in preview pixels, <see cref="BezelImage"/> was last rendered at, so
    /// it's only re-rendered when the screen's size actually changes.
    /// </summary>
    public (int Width, int Height) RenderedBezelSize
    {
        get;
        set;
    }
}
