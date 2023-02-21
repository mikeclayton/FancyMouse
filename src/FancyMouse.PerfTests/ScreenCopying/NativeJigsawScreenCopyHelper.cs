using FancyMouse.PerfTests.Helpers;
using FancyMouse.PerfTests.NativeMethods;
using FancyMouse.PerfTests.NativeMethods.Core;

namespace FancyMouse.PerfTests.ScreenCopying;

public sealed class NativeJigsawScreenCopyHelper : IScreenCopyHelper
{

    public Bitmap CopyFromScreen(
        Rectangle desktopBounds, IEnumerable<Rectangle> desktopRegions, Size screenshotSize
    )
    {

        // based on https://www.cyotek.com/blog/capturing-screenshots-using-csharp-and-p-invoke

        var desktopHwnd = HWND.Null;
        var desktopHdc = HDC.Null;
        var screenshotHdc = HDC.Null;
        var screenshotHBitmap = HBITMAP.Null;
        var originalHBitmap = HGDIOBJ.Null;

        var screenshot = default(Bitmap);

        try
        {

            desktopHwnd = User32.GetDesktopWindow();
            desktopHdc = NativeWrappers.GetWindowDC(desktopHwnd);
            screenshotHdc = NativeWrappers.CreateCompatibleDC(desktopHdc);
            screenshotHBitmap = NativeWrappers.CreateCompatibleBitmap(desktopHdc, screenshotSize.Width, screenshotSize.Height);
            originalHBitmap = NativeWrappers.SelectObject(screenshotHdc, screenshotHBitmap);
            _ = NativeWrappers.SetStretchBltMode(screenshotHdc, Gdi32.STRETCH_BLT_MODE.STRETCH_HALFTONE);

            var scalingRatio = LayoutHelper.GetScalingRatio(desktopBounds.Size, screenshotSize);
            foreach (var desktopRegion in desktopRegions)
            {
                var screenshotRegion = new Rectangle(
                    x: (int)((desktopRegion.X - desktopBounds.X) * scalingRatio),
                    y: (int)((desktopRegion.Y - desktopBounds.Y) * scalingRatio),
                    width: (int)(desktopRegion.Width * scalingRatio),
                    height: (int)(desktopRegion.Height * scalingRatio)
                );
                _ = NativeWrappers.StretchBlt(
                    screenshotHdc, screenshotRegion.X, screenshotRegion.Y, screenshotRegion.Width, screenshotRegion.Height,
                    desktopHdc, desktopRegion.X, desktopRegion.Y, desktopRegion.Width, desktopRegion.Height,
                    Gdi32.ROP_CODE.SRCCOPY
                );
            }

            screenshot = Bitmap.FromHbitmap(screenshotHBitmap.Value);

        }
        finally
        {
            if (!screenshotHdc.IsNull && !originalHBitmap.IsNull)
            {
                _ = NativeWrappers.SelectObject(screenshotHdc, originalHBitmap);
            }
            if (!screenshotHBitmap.IsNull)
            {
                _ = NativeWrappers.DeleteObject(screenshotHBitmap);
            }
            if (!screenshotHdc.IsNull)
            {
                _ = NativeWrappers.DeleteDC(screenshotHdc);
            }
            if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
            {
                _ = NativeWrappers.ReleaseDC(desktopHwnd, desktopHdc);
            }
        }

        return screenshot ?? throw new InvalidOperationException();

    }

}
