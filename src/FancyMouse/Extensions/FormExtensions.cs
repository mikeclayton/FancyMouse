namespace FancyMouse.Extensions;

internal static class FormExtensions
{

    public static Point GetCenteredChildLocation(this Form parent, Form child)
    {
        return new Point(
            parent.Location.X + (int)((float)(parent.Size.Width - child.Size.Width) / 2),
            parent.Location.Y + (int)((float)(parent.Size.Height - child.Size.Height) / 2)
        );
    }


}
