using System.Drawing;
using System.Drawing.Imaging;
using FancyMouse.Helpers;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Settings;
using FancyMouse.Models.Styles;
using FancyMouse.NativeMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FancyMouse.UnitTests.Helpers;

[TestClass]
public static class DrawingHelperTests
{
    [TestClass]
    public sealed class GetPreviewLayoutTests
    {
        private static Bitmap DrawDesktopImage(List<(RectangleInfo Bounds, Color Color)> screens)
        {
            // created a "desktop" bitmap with the given screen areas drawn the specified colors.
            var desktopBounds = LayoutHelper.GetCombinedScreenBounds(
                screens.Select(s => s.Bounds).ToList());

            // we can only create bitmaps with non-negative coordinates
            if (desktopBounds.X < 0 || desktopBounds.Y < 0)
            {
                throw new InvalidOperationException();
            }

            // create the bitmap
            var bitmap = new Bitmap((int)desktopBounds.Width, (int)desktopBounds.Height, PixelFormat.Format32bppArgb);

            // draw the rectangles
            using var graphics = Graphics.FromImage(bitmap);
            foreach (var screen in screens)
            {
                var screenBounds = screen.Bounds.ToRectangle();
                /*
                using var pen = new Pen(screen.Color);
                graphics.DrawRectangle(
                    pen, screenBounds.X, screenBounds.Y, screenBounds.Width - 1, screenBounds.Height - 1);
                */
                using var brush = new SolidBrush(screen.Color);
                graphics.FillRectangle(brush, screenBounds);
            }

            return bitmap;
        }

        public sealed class TestCase
        {
            public TestCase(PreviewStyle previewStyle, List<(RectangleInfo Bounds, Color Color)> screens, PointInfo activatedLocation)
            {
                this.PreviewStyle = previewStyle;
                this.Screens = screens;
                this.ActivatedLocation = activatedLocation;
            }

            public PreviewStyle PreviewStyle { get; }

            public List<(RectangleInfo Bounds, Color Color)> Screens { get; }

            public PointInfo ActivatedLocation { get; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            yield return new object[]
            {
                new TestCase(
                    previewStyle: AppSettings.DefaultSettings.PreviewStyle,
                    screens: new()
                    {
                        new()
                        {
                            Bounds = new RectangleInfo(0, 0, 500, 500),
                            Color = Color.Red,
                        },
                        new()
                        {
                            Bounds = new RectangleInfo(500, 0, 500, 500),
                            Color = Color.Orange,
                        },
                        new()
                        {
                            Bounds = new RectangleInfo(500, 500, 500, 500),
                            Color = Color.Yellow,
                        },
                        new()
                        {
                            Bounds = new RectangleInfo(0, 500, 500, 500),
                            Color = Color.Green,
                        },
                    },
                    new(x: 50, y: 50)),
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void RunTestCases(TestCase data)
        {
            // generate the fake desktop image
            using var desktop = GetPreviewLayoutTests.DrawDesktopImage(data.Screens);

            // var desktop = Bitmap.FromFile(".\\desktop.png");
            using var graphics = Graphics.FromImage(desktop);
            var desktopHdc = new Core.HDC(graphics.GetHdc());

            // draw the preview image
            var previewLayout = LayoutHelper.GetPreviewLayout(
                previewStyle: data.PreviewStyle,
                screens: data.Screens.Select(s => s.Bounds).ToList(),
                activatedLocation: data.ActivatedLocation);
            using var actual = DrawingHelper.RenderPreview(previewLayout, desktopHdc);
        }
    }
}
