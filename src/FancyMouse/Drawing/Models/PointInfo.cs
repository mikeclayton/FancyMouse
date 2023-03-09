﻿namespace FancyMouse.Drawing.Models;

/// <summary>
/// Immutable version of a System.Drawing.Point object with some extra utility methods.
/// </summary>
internal sealed class PointInfo
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
    }

    public decimal Y
    {
        get;
    }

    public SizeInfo Size => new((int)this.X, (int)this.Y);

    public PointInfo Scale(decimal scalingFactor) => new(this.X * scalingFactor, this.Y * scalingFactor);

    public Point ToPoint() => new((int)this.X, (int)this.Y);

    public override string ToString()
    {
        return "{" +
            $"{nameof(this.X)}={this.X}," +
            $"{nameof(this.Y)}={this.Y}" +
            "}";
    }
}
