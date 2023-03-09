using System.Drawing.Imaging;
using FancyMouse.Drawing.Models;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.PerfTests.Drawing;

internal static class ParallelStretchBltScreenCopyHelper
{
    public static Bitmap CopyFromScreen(
        RectangleInfo desktopBounds, IEnumerable<RectangleInfo> desktopRegions, SizeInfo screenshotSize)
    {
        // based on https://www.cyotek.com/blog/capturing-screenshots-using-csharp-and-p-invoke
        var scalingRatio = desktopBounds.Size.ScaleToFitRatio(screenshotSize);
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 1,
        };

        var screenBoundsList = desktopRegions.ToArray();

        var screenshot = new Bitmap(
            (int)screenshotSize.Width, (int)screenshotSize.Height, PixelFormat.Format32bppArgb);

        using var graphics = Graphics.FromImage(screenshot);

        Parallel.For(
            0,
            screenBoundsList.Length,
            parallelOptions,
            (i, state) =>
            {
                var screenRegion = screenBoundsList[i];
                var scaledRegion = new Rectangle(
                    x: (int)((screenRegion.X - desktopBounds.X) * scalingRatio),
                    y: (int)((screenRegion.Y - desktopBounds.Y) * scalingRatio),
                    width: (int)(screenRegion.Width * scalingRatio),
                    height: (int)(screenRegion.Height * scalingRatio));
                using var scaledImage = ParallelStretchBltScreenCopyHelper.CopyScreen(desktopBounds, screenRegion, new(scaledRegion.Size));
                graphics.DrawImage(scaledImage, scaledRegion.Location);
            });

        return screenshot;
    }

    private static Bitmap CopyScreen(RectangleInfo desktopBounds, RectangleInfo screenBounds, SizeInfo scaledSize)
    {
        var desktopHwnd = HWND.Null;
        var desktopHdc = HDC.Null;

        var screenshotHdc = CreatedHDC.Null;
        var screenshotHBitmap = HBITMAP.Null;
        var originalHBitmap = HGDIOBJ.Null;

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
                desktopHdc,
                (int)scaledSize.Width,
                (int)scaledSize.Height);
            if (screenshotHBitmap.IsNull)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.CreateCompatibleBitmap)} returned null");
            }

            originalHBitmap = PInvoke.SelectObject(new HDC(screenshotHdc), (HGDIOBJ)screenshotHBitmap.Value);
            if (originalHBitmap.IsNull)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.SelectObject)} returned null");
            }

            apiResult = PInvoke.SetStretchBltMode(new HDC(screenshotHdc), STRETCH_BLT_MODE.STRETCH_HALFTONE);
            if (apiResult == 0)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.SetStretchBltMode)} returned {apiResult}");
            }

            apiResult = PInvoke.StretchBlt(
                new HDC(screenshotHdc),
                0,
                0,
                (int)scaledSize.Width,
                (int)scaledSize.Height,
                desktopHdc,
                (int)screenBounds.X,
                (int)screenBounds.Y,
                (int)screenBounds.Width,
                (int)screenBounds.Height,
                ROP_CODE.SRCCOPY);
            if (apiResult == 0)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.StretchBlt)} returned {apiResult}");
            }

            return Bitmap.FromHbitmap(screenshotHBitmap);
        }
        finally
        {
            if (!screenshotHdc.IsNull && !originalHBitmap.IsNull)
            {
                PInvoke.SelectObject(new HDC(screenshotHdc), originalHBitmap);
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
                apiResult = PInvoke.DeleteDC(screenshotHdc);
                if (apiResult == 0)
                {
                    // throw new InvalidOperationException($"{nameof(PInvoke.DeleteDC)} returned {apiResult}");
                }
            }

            if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
            {
                apiResult = PInvoke.ReleaseDC(desktopHwnd, desktopHdc);
                if (apiResult == 0)
                {
                    // throw new InvalidOperationException($"{nameof(PInvoke.ReleaseDC)} returned {apiResult}");
                }
            }
        }
    }
}
