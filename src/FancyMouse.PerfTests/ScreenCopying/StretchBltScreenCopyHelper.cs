using FancyMouse.ScreenCopying;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace FancyMouse.PerfTests.ScreenCopying;

public sealed class StretchBltScreenCopyHelper : ICopyFromScreen
{

    public Bitmap CopyFromScreen(
        Rectangle desktopBounds, IEnumerable<Rectangle> desktopRegions, Size screenshotSize
    )
    {

        /// based on https://www.cyotek.com/blog/capturing-screenshots-using-csharp-and-p-invoke

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

            screenshotHBitmap = PInvoke.CreateCompatibleBitmap(desktopHdc, screenshotSize.Width, screenshotSize.Height);
            if (screenshotHBitmap.IsNull)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.CreateCompatibleBitmap)} returned null");
            }

            originalHBitmap = PInvoke.SelectObject(new HDC(screenshotHdc), (HGDIOBJ)screenshotHBitmap.Value);

            apiResult = PInvoke.SetStretchBltMode(new HDC(screenshotHdc), STRETCH_BLT_MODE.STRETCH_HALFTONE);
            if (apiResult == 0)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.SetStretchBltMode)} returned {apiResult}");
            }

            apiResult = PInvoke.StretchBlt(
                new HDC(screenshotHdc), 0, 0, screenshotSize.Width, screenshotSize.Height,
                desktopHdc, 0, 0, desktopBounds.Width, desktopBounds.Height,
                ROP_CODE.SRCCOPY
            );
            if (apiResult == 0)
            {
                throw new InvalidOperationException($"{nameof(PInvoke.StretchBlt)} returned {apiResult}");
            }

            screenshot = Bitmap.FromHbitmap(screenshotHBitmap);

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
                    throw new InvalidOperationException($"{nameof(PInvoke.DeleteDC)} returned {apiResult}");
                }
            }

            if (!screenshotHdc.IsNull)
            {
                apiResult = PInvoke.DeleteDC(screenshotHdc);
                if (apiResult == 0)
                {
                    throw new InvalidOperationException($"{nameof(PInvoke.DeleteDC)} returned {apiResult}");
                }
            }

            if (!desktopHwnd.IsNull && !desktopHdc.IsNull)
            {
                apiResult = PInvoke.ReleaseDC(desktopHwnd, desktopHdc);
                if (apiResult == 0)
                {
                    throw new InvalidOperationException($"{nameof(PInvoke.ReleaseDC)} returned {apiResult}");
                }
            }

        }

        return screenshot ?? throw new InvalidOperationException();

    }

}
