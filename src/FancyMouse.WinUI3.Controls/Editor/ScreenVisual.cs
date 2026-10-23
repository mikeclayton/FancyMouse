using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Image = Microsoft.UI.Xaml.Controls.Image;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace FancyMouse.WinUI3.Controls.Editor;

/// <summary>
/// The UI elements <see cref="ScreenLayoutEditor"/> maintains for one screen - a rounded bezel
/// ring, a fill inside it, and the screen's number and resolution, all in one container so the
/// whole screen moves, resizes and restacks as a unit.
/// </summary>
internal sealed class ScreenVisual
{
    public ScreenVisual(EditableScreen screen, CursorGrid container, Image bezelImage, Rectangle fill, TextBlock number, TextBlock caption)
    {
        this.Screen = screen;
        this.Container = container;
        this.BezelImage = bezelImage;
        this.Fill = fill;
        this.Number = number;
        this.Caption = caption;
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

    public TextBlock Number
    {
        get;
    }

    public TextBlock Caption
    {
        get;
    }

    /// <summary>
    /// Gets or sets the physical pixel size <see cref="BezelImage"/> was last rendered at, so it's
    /// only re-rendered when the screen's size on screen actually changes.
    /// </summary>
    public (int Width, int Height) RenderedBezelSize
    {
        get;
        set;
    }

    public void SetSelected(bool isSelected, ResourceDictionary resources)
    {
        this.Fill.Style = (Style)resources[isSelected ? "SelectedScreenFillStyle" : "ScreenFillStyle"];
        this.Number.Style = (Style)resources[isSelected ? "SelectedScreenNumberStyle" : "ScreenNumberStyle"];
        this.Caption.Style = (Style)resources[isSelected ? "SelectedScreenCaptionStyle" : "ScreenCaptionStyle"];
    }
}
