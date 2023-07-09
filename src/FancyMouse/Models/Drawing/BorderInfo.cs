﻿namespace FancyMouse.Models.Drawing;

public sealed class BorderInfo
{
    public static readonly BorderInfo Empty = new(Color.Transparent, 0, 0);

    public BorderInfo(Color color, decimal all, decimal depth)
        : this(color, all, all, all, all, depth)
    {
    }

    public BorderInfo(Color color, decimal left, decimal top, decimal right, decimal bottom, decimal depth)
    {
        this.Color = color;
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
        this.Depth = depth;
    }

    public Color Color
    {
        get;
    }

    public decimal Left
    {
        get;
    }

    public decimal Top
    {
        get;
    }

    public decimal Right
    {
        get;
    }

    public decimal Bottom
    {
        get;
    }

    /// <summary>
    /// Gets the "depth" of the 3d highlight and shadow effect on the border.
    /// </summary>
    public decimal Depth
    {
        get;
    }

    public decimal Horizontal => this.Left + this.Right;

    public decimal Vertical => this.Top + this.Bottom;

    public override string ToString()
    {
        return "{" +
           $"{nameof(this.Color)}={this.Color}" +
           $"{nameof(this.Left)}={this.Left}," +
           $"{nameof(this.Top)}={this.Top}," +
           $"{nameof(this.Right)}={this.Right}," +
           $"{nameof(this.Bottom)}={this.Bottom}" +
           "}";
    }
}
