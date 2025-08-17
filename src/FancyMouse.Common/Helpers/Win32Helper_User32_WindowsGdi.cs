using System.ComponentModel;
using System.Runtime.InteropServices;

using static FancyMouse.Common.NativeMethods.Core;

namespace FancyMouse.Common.Helpers;

public static partial class Win32Helper
{
    public static partial class User32
    {
        /// <summary>
        /// The MapWindowPoints function converts (maps) a set of points from a
        /// coordinate space relative to one window to a coordinate space relative to another window.
        /// </summary>
        public static POINT MapWindowPoint(HWND hWndFrom, HWND hWndTo, POINT point)
        {
            var cPoints = (UINT)1;
            var lpPoint = new LPPOINT(point);
            var result = NativeMethods.User32.MapWindowPoints(hWndFrom, hWndTo, lpPoint, cPoints);
            if (result == 0)
            {
                var lastError = Marshal.GetLastPInvokeError();
                throw new Win32Exception(lastError);
            }

            var mappedPoint = lpPoint.ToStructure();
            lpPoint.Free();

            return mappedPoint;
        }
    }
}
