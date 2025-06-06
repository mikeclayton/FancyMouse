﻿using System.Drawing;

namespace FancyMouse.Common.Models.Drawing;

/// <summary>
/// Immutable version of a System.Drawing.Point object with some extra utility methods.
/// </summary>
public sealed record PointInfo
{
    public PointInfo(decimal x, decimal y)
    {
        this.X = x;
        this.Y = y;
    }

    public PointInfo(Point point)
        : this(point.X, point.Y)
    {
    }

    public decimal X
    {
        get;
        init;
    }

    public decimal Y
    {
        get;
        init;
    }

    /// <summary>
    /// Moves this PointInfo inside the specified RectangleInfo.
    /// </summary>
    public PointInfo Clamp(RectangleInfo outer)
    {
        return new(
            x: Math.Clamp(this.X, outer.X, outer.Right),
            y: Math.Clamp(this.Y, outer.Y, outer.Bottom));
    }

    public PointInfo Offset(PointInfo amount) => new(this.X + amount.X, this.Y + amount.Y);

    public PointInfo Offset(SizeInfo amount) => new(this.X + amount.Width, this.Y + amount.Height);

    public PointInfo Offset(decimal amountX, decimal amountY) => new(this.X + amountX, this.Y + amountY);

    public PointInfo Scale(decimal scalingFactor) => new(this.X * scalingFactor, this.Y * scalingFactor);

    /// <summary>
    /// Stretches the point to the same proportional position in targetBounds as
    /// it currently is in sourceBounds
    /// </summary>
    public PointInfo Stretch(RectangleInfo source, RectangleInfo target) =>
        new PointInfo(
            x: ((this.X - source.X) / source.Width * target.Width) + target.X,
            y: ((this.Y - source.Y) / source.Height * target.Height) + target.Y);

    /// <summary>
    /// Subtracts the specified point from the current point.
    /// </summary>
    public PointInfo Subtract(PointInfo point) =>
        new(
            x: this.X - point.X,
            y: this.Y - point.Y);

    public Point ToPoint() => new((int)this.X, (int)this.Y);

    public SizeInfo ToSize()
    {
        return new((int)this.X, (int)this.Y);
    }

    public PointInfo Truncate() =>
        new(
            (int)this.X,
            (int)this.Y);

    public override string ToString()
    {
        return "{" +
            $"{nameof(this.X)}={this.X}," +
            $"{nameof(this.Y)}={this.Y}" +
            "}";
    }
}
