using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.Editor;

/// <summary>
/// The editor's camera - maps preview pixels to a view's own units as a uniform zoom plus an
/// offset, and keeps itself over a virtual canvas so the user can't lose the preview off into space.
/// </summary>
public sealed class EditorCamera
{
    /// <summary>
    /// The most the camera can zoom in, in view units per preview pixel.
    /// </summary>
    public const double MaximumZoom = 8;

    public EditorCamera(RectangleInfo canvasBounds, RectangleInfo fitBounds, double fitPadding)
    {
        this.CanvasBounds = canvasBounds ?? throw new ArgumentNullException(nameof(canvasBounds));
        this.FitBounds = fitBounds ?? throw new ArgumentNullException(nameof(fitBounds));
        this.FitPadding = fitPadding;
        this.Zoom = 1;
    }

    /// <summary>
    /// Gets the virtual canvas the camera roams over, in preview pixels.
    /// </summary>
    public RectangleInfo CanvasBounds
    {
        get;
    }

    /// <summary>
    /// Gets the area <see cref="ZoomToFit"/> points the camera at, in preview pixels.
    /// </summary>
    public RectangleInfo FitBounds
    {
        get;
    }

    /// <summary>
    /// Gets the space, in view units, <see cref="ZoomToFit"/> keeps clear around <see cref="FitBounds"/>.
    /// </summary>
    public double FitPadding
    {
        get;
    }

    public double ViewportWidth
    {
        get;
        private set;
    }

    public double ViewportHeight
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the zoom, in view units per preview pixel.
    /// </summary>
    public double Zoom
    {
        get;
        private set;
    }

    public double OffsetX
    {
        get;
        private set;
    }

    public double OffsetY
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets a value indicating whether the user has zoomed or panned since the last
    /// <see cref="ZoomToFit"/> - until they have, resizing the viewport re-fits rather than
    /// leaving the camera where it was.
    /// </summary>
    public bool IsMoved
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the most the camera can zoom out - the whole virtual canvas fitting in the viewport.
    /// </summary>
    public double MinimumZoom
        => Math.Min(
            this.ViewportWidth / (double)this.CanvasBounds.Width,
            this.ViewportHeight / (double)this.CanvasBounds.Height);

    private bool HasViewport => (this.ViewportWidth > 0) && (this.ViewportHeight > 0);

    public void SetViewportSize(double width, double height)
    {
        this.ViewportWidth = width;
        this.ViewportHeight = height;
        if (this.IsMoved)
        {
            this.Clamp();
        }
        else
        {
            this.ZoomToFit();
        }
    }

    /// <summary>
    /// Points the camera at the whole of <see cref="FitBounds"/>, as large as fits in the viewport.
    /// </summary>
    public void ZoomToFit()
    {
        this.IsMoved = false;
        if (!this.HasViewport)
        {
            return;
        }

        var fit = this.FitBounds;
        var availableWidth = Math.Max(1, this.ViewportWidth - (2 * this.FitPadding));
        var availableHeight = Math.Max(1, this.ViewportHeight - (2 * this.FitPadding));
        this.Zoom = Math.Min(availableWidth / (double)fit.Width, availableHeight / (double)fit.Height);
        this.OffsetX = ((this.ViewportWidth - ((double)fit.Width * this.Zoom)) / 2) - ((double)fit.X * this.Zoom);
        this.OffsetY = ((this.ViewportHeight - ((double)fit.Height * this.Zoom)) / 2) - ((double)fit.Y * this.Zoom);
    }

    /// <summary>
    /// Zooms by <paramref name="factor"/>, keeping the preview pixel under (<paramref name="viewX"/>,
    /// <paramref name="viewY"/>) where it is - so the wheel zooms in on whatever the pointer is over.
    /// </summary>
    public void ZoomAt(double viewX, double viewY, double factor)
    {
        var (previewX, previewY) = this.ToPreview(viewX, viewY);
        this.Zoom *= factor;
        this.OffsetX = viewX - (previewX * this.Zoom);
        this.OffsetY = viewY - (previewY * this.Zoom);
        this.IsMoved = true;
        this.Clamp();
    }

    /// <summary>
    /// Zooms to <paramref name="zoom"/> about the middle of the viewport (within the zoom limits).
    /// </summary>
    public void ZoomTo(double zoom)
        => this.ZoomAt(this.ViewportWidth / 2, this.ViewportHeight / 2, zoom / this.Zoom);

    public void PanBy(double deltaX, double deltaY)
    {
        this.OffsetX += deltaX;
        this.OffsetY += deltaY;
        this.IsMoved = true;
        this.Clamp();
    }

    public ViewRect ToView(double x, double y, double width, double height)
        => new(
            this.OffsetX + (x * this.Zoom),
            this.OffsetY + (y * this.Zoom),
            width * this.Zoom,
            height * this.Zoom);

    public ViewRect ToView(RectangleInfo bounds)
    {
        ArgumentNullException.ThrowIfNull(bounds);
        return this.ToView((double)bounds.X, (double)bounds.Y, (double)bounds.Width, (double)bounds.Height);
    }

    public ViewRect ToView(EditableScreen screen)
    {
        ArgumentNullException.ThrowIfNull(screen);
        return this.ToView(screen.X, screen.Y, screen.Width, screen.Height);
    }

    public (double X, double Y) ToPreview(double viewX, double viewY)
        => ((viewX - this.OffsetX) / this.Zoom, (viewY - this.OffsetY) / this.Zoom);

    /// <summary>
    /// Keeps the camera on the virtual canvas - zoomed out no further than <see cref="MinimumZoom"/>,
    /// zoomed in no further than <see cref="MaximumZoom"/>, and with the middle of the viewport
    /// always somewhere over the canvas.
    /// </summary>
    private void Clamp()
    {
        if (!this.HasViewport)
        {
            return;
        }

        var maximumZoom = Math.Max(this.MinimumZoom, EditorCamera.MaximumZoom);
        var zoom = Math.Clamp(this.Zoom, this.MinimumZoom, maximumZoom);

        // re-apply the zoom about the middle of the viewport, then nudge the middle back over the canvas
        var (middleViewX, middleViewY) = (this.ViewportWidth / 2, this.ViewportHeight / 2);
        var (middleX, middleY) = this.ToPreview(middleViewX, middleViewY);
        var canvas = this.CanvasBounds;
        middleX = Math.Clamp(middleX, (double)canvas.Left, (double)canvas.Right);
        middleY = Math.Clamp(middleY, (double)canvas.Top, (double)canvas.Bottom);

        this.Zoom = zoom;
        this.OffsetX = middleViewX - (middleX * zoom);
        this.OffsetY = middleViewY - (middleY * zoom);
    }
}
