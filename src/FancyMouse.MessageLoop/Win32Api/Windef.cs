namespace FancyMouse.MessageLoop.Win32Api;

internal static class Windef
{

    #region Structures

    /// <summary>
    /// The POINT structure defines the x- and y- coordinates of a point.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-us/previous-versions/dd162805(v=vs.85)
    /// </remarks>
    public struct POINT
    {
        int X;
        int y;
    }

    #endregion

}
