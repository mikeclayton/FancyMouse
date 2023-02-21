using FancyMouse.Helpers;
using FancyMouse.NativeWrappers;
using FancyMouse.NativeMethods.Core;

namespace FancyMouse.ScreenCopying;

public sealed class NativeJigsawScreenCopyHelper : ICopyFromScreen
{

    public Bitmap CopyFromScreen(
        Rectangle desktopBounds, IEnumerable<Rectangle> desktopRegions, Size screenshotSize
    )
    {

        // based on https://www.cyotek.com/blog/capturing-screenshots-using-csharp-and-p-invoke

        // note - it's faster to capture each monitor individually and assemble them into
        // a single image ourselves as we *may* not have to transfer all of the blank pixels
        // that are outside the desktop bounds - e.g. the *** in the ascii art below
        //
        // +----------------+********
        // |                |********
        // |       1        +-------+
        // |                |       |
        // +----------------+   0   |
        // *****************|       |
        // *****************+-------+
        //
        // for very irregular monitor layouts this *might* be a big percentage of the rectangle
        // containing the desktop bounds.

        var desktopHwnd = HWND.Null;
        var desktopHdc = HDC.Null;
        var screenshotHdc = HDC.Null;
        var screenshotHBitmap = HBITMAP.Null;
        var originalHBitmap = HGDIOBJ.Null;

        var screenshot = default(Bitmap);

        try
        {

            desktopHwnd = User32.GetDesktopWindow();
            desktopHdc = User32.GetWindowDC(desktopHwnd);
            screenshotHdc = Gdi32.CreateCompatibleDC(desktopHdc);
            screenshotHBitmap = Gdi32.CreateCompatibleBitmap(desktopHdc, screenshotSize.Width, screenshotSize.Height);
            originalHBitmap = Gdi32.SelectObject(screenshotHdc, screenshotHBitmap);
            _ = Gdi32.SetStretchBltMode(screenshotHdc, NativeMethods.Gdi32.STRETCH_BLT_MODE.STRETCH_HALFTONE);

            var scalingRatio = LayoutHelper.GetScalingRatio(desktopBounds.Size, screenshotSize);
            foreach (var desktopRegion in desktopRegions)
            {
                var screenshotRegion = new Rectangle(
                    x: (int)((desktopRegion.X - desktopBounds.X) * scalingRatio),
                    y: (int)((desktopRegion.Y - desktopBounds.Y) * scalingRatio),
                    width: (int)(desktopRegion.Width * scalingRatio),
                    height: (int)(desktopRegion.Height * scalingRatio)
                );
                _ = Gdi32.StretchBlt(
                    screenshotHdc, screenshotRegion.X, screenshotRegion.Y, screenshotRegion.Width, screenshotRegion.Height,
                    desktopHdc, desktopRegion.X, desktopRegion.Y, desktopRegion.Width, desktopRegion.Height,
                    NativeMethods.Gdi32.ROP_CODE.SRCCOPY
                );
            }

            screenshot = Bitmap.FromHbitmap(screenshotHBitmap.Value);

        }
        finally
        {
            if (!screenshotHdc.IsNull && !originalHBitmap.IsNull)
            {
                _ = Gdi32.SelectObject(screenshotHdc, originalHBitmap);
            }
            if (!screenshotHBitmap.IsNull)
            {
                _ = Gdi32.DeleteObject(screenshotHBitmap);
            }
            if (!screenshotHdc.IsNull)
            {
                _ = Gdi32.DeleteDC(screenshotHdc);
            }
            if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
            {
                _ = User32.ReleaseDC(desktopHwnd, desktopHdc);
            }
        }

        return screenshot ?? throw new InvalidOperationException();

    }

}
