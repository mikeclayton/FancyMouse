using static FancyMouse.PlatformServices.Windows.NativeMethods.Core;

namespace FancyMouse.PlatformServices.Windows.Interop;

internal static partial class User32
{
    public static POINT GetCursorPos()
    {
        var lpPoint = new LPPOINT(new POINT(0, 0));
        var result = NativeMethods.User32.GetCursorPos(lpPoint);
        ResultHandler.ThrowIfNotZero(result.Value, getLastError: true);

        var point = lpPoint.ToStructure();
        lpPoint.Free();

        return point;
    }

    public static BOOL SetCursorPos(int x, int y)
    {
        var result = NativeMethods.User32.SetCursorPos(x, y);
        ResultHandler.ThrowIfNotZero(result.Value, getLastError: true);

        return result;
    }
}
