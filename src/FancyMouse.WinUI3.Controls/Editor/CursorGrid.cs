using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace FancyMouse.WinUI3.Controls.Editor;

/// <summary>
/// A <see cref="Grid"/> that shows a particular mouse cursor while the pointer is over it - the
/// "move" cursor over a screen, and the diagonal resize cursors over its corner handles.
/// </summary>
/// <remarks>
/// <c>UIElement.ProtectedCursor</c> can only be set from a derived class, hence this tiny subclass.
/// </remarks>
internal sealed partial class CursorGrid : Grid
{
    public CursorGrid(InputSystemCursorShape cursorShape)
    {
        this.ProtectedCursor = InputSystemCursor.Create(cursorShape);
    }
}
