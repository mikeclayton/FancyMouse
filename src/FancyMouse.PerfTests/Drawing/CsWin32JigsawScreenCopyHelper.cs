using FancyMouse.Drawing.Models;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.PerfTests.Drawing;

internal static class CsWin32JigsawScreenCopyHelper
{
    public static Bitmap CopyFromScreen(
        RectangleInfo desktopBounds, IEnumerable<RectangleInfo> desktopRegions, SizeInfo screenshotSize)
    {
        // based on https://www.cyotek.com/blog/capturing-screenshots-using-csharp-and-p-invoke
        var desktopHwnd = HWND.Null;
        var desktopHdc = HDC.Null;
        var screenshotHdc = CreatedHDC.Null;
        var screenshotHBitmap = HBITMAP.Null;
        var originalHBitmap = HGDIOBJ.Null;

        var screenshot = default(Bitmap);

        var apiResult = default(int);

        try
        {
            desktopHwnd = PInvoke.GetDesktopWindow();

            desktopHdc = PInvoke.GetWindowDC(desktopHwnd);
            if (desktopHdc.IsNull)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.GetWindowDC)} returned null");
            }

            screenshotHdc = PInvoke.CreateCompatibleDC(desktopHdc);
            if (screenshotHdc.IsNull)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.CreateCompatibleDC)} returned null");
            }

            screenshotHBitmap = PInvoke.CreateCompatibleBitmap(
                desktopHdc, (int)screenshotSize.Width, (int)screenshotSize.Height);
            if (screenshotHBitmap.IsNull)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.CreateCompatibleBitmap)} returned null");
            }

            originalHBitmap = PInvoke.SelectObject(new HDC(screenshotHdc.Value), (HGDIOBJ)screenshotHBitmap.Value);
            if (originalHBitmap.IsNull)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.SelectObject)} returned null");
            }

            apiResult = PInvoke.SetStretchBltMode(new HDC(screenshotHdc.Value), STRETCH_BLT_MODE.STRETCH_HALFTONE);
            if (apiResult == 0)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.SetStretchBltMode)} returned {apiResult}");
            }

            var scalingRatio = desktopBounds.Size.ScaleToFitRatio(screenshotSize);
            foreach (var desktopRegion in desktopRegions)
            {
                var screenshotRegion = new Rectangle(
                    x: (int)((desktopRegion.X - desktopBounds.X) * scalingRatio),
                    y: (int)((desktopRegion.Y - desktopBounds.Y) * scalingRatio),
                    width: (int)(desktopRegion.Width * scalingRatio),
                    height: (int)(desktopRegion.Height * scalingRatio));
                apiResult = PInvoke.StretchBlt(
                    new HDC(screenshotHdc.Value),
                    screenshotRegion.X,
                    screenshotRegion.Y,
                    screenshotRegion.Width,
                    screenshotRegion.Height,
                    new HDC(desktopHdc.Value),
                    (int)desktopRegion.X,
                    (int)desktopRegion.Y,
                    (int)desktopRegion.Width,
                    (int)desktopRegion.Height,
                    ROP_CODE.SRCCOPY);
                if (apiResult == 0)
                {
                    throw new InvalidOperationException($"{nameof(PInvoke.StretchBlt)} returned {apiResult}");
                }
            }

            screenshot = Bitmap.FromHbitmap(screenshotHBitmap.Value);
        }
        finally
        {
            if (!screenshotHdc.IsNull && !originalHBitmap.IsNull)
            {
                PInvoke.SelectObject(new HDC(screenshotHdc.Value), originalHBitmap);
            }

            if (!screenshotHBitmap.IsNull)
            {
                apiResult = PInvoke.DeleteObject((HGDIOBJ)screenshotHBitmap.Value);
                if (apiResult == 0)
                {
                    // throw new InvalidOperationException($"{nameof(PInvoke.DeleteDC)} returned {apiResult}");
                }
            }

            if (!screenshotHdc.IsNull)
            {
                apiResult = PInvoke.DeleteDC(new CreatedHDC(screenshotHdc.Value));
                if (apiResult == 0)
                {
                    // throw new InvalidOperationException($"{nameof(PInvoke.DeleteDC)} returned {apiResult}");
                }
            }

            if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
            {
                apiResult = PInvoke.ReleaseDC(new HWND(desktopHwnd.Value), new HDC(desktopHdc.Value));
                if (apiResult == 0)
                {
                    // throw new InvalidOperationException($"{nameof(PInvoke.ReleaseDC)} returned {apiResult}");
                }
            }
        }

        return screenshot ?? throw new InvalidOperationException();
    }
}
