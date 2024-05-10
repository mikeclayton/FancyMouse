using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using FancyMouse.Common.Helpers;
using FancyMouse.Common.Imaging;
using FancyMouse.Common.Models.Drawing;
using FancyMouse.Common.Models.Styles;
using FancyMouse.Internal.Models.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FancyMouse.UnitTests.Common.Helpers;

[TestClass]
public static class DrawingHelperTests
{
    [TestClass]
    public sealed class GetPreviewLayoutTests
    {
        public sealed class TestCase
        {
            public TestCase(PreviewStyle previewStyle, List<RectangleInfo> screens, PointInfo activatedLocation, string desktopImageFilename, string expectedImageFilename)
            {
                this.PreviewStyle = previewStyle;
                this.Screens = screens;
                this.ActivatedLocation = activatedLocation;
                this.DesktopImageFilename = desktopImageFilename;
                this.ExpectedImageFilename = expectedImageFilename;
            }

            public PreviewStyle PreviewStyle { get; }

            public List<RectangleInfo> Screens { get; }

            public PointInfo ActivatedLocation { get; }

            public string DesktopImageFilename { get; }

            public string ExpectedImageFilename { get; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            /* 4-grid */
            yield return new object[]
            {
                new TestCase(
                    previewStyle: AppSettings.DefaultSettings.PreviewStyle,
                    screens: new List<RectangleInfo>()
                    {
                        new(0, 0, 500, 500),
                        new(500, 0, 500, 500),
                        new(500, 500, 500, 500),
                        new(0, 500, 500, 500),
                    },
                    activatedLocation: new(x: 50, y: 50),
                    desktopImageFilename: "Common/Helpers/_test-4grid-desktop.png",
                    expectedImageFilename: "Common/Helpers/_test-4grid-expected.png"),
            };
            /* win 11 */
            yield return new object[]
            {
                new TestCase(
                    previewStyle: AppSettings.DefaultSettings.PreviewStyle,
                    screens: new List<RectangleInfo>()
                    {
                        new(5120, 349, 1920, 1080),
                        new(0, 0, 5120, 1440),
                    },
                    activatedLocation: new(x: 50, y: 50),
                    desktopImageFilename: "Common/Helpers/_test-win11-desktop.png",
                    expectedImageFilename: "Common/Helpers/_test-win11-expected.png"),
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void RunTestCases(TestCase data)
        {
            // load the fake desktop image
            using var desktopImage = GetPreviewLayoutTests.LoadImageResource(data.DesktopImageFilename);

            // draw the preview image
            var previewLayout = LayoutHelper.GetPreviewLayout(
                previewStyle: data.PreviewStyle,
                screens: data.Screens,
                activatedLocation: data.ActivatedLocation);
            var imageCopyService = new StaticImageRegionCopyService(desktopImage);
            using var actual = DrawingHelper.RenderPreview(previewLayout, imageCopyService);

            // save the actual image so we can pick it up as a build artifact
            var actualFilename = Path.GetFileNameWithoutExtension(data.ExpectedImageFilename) + "_actual" + Path.GetExtension(data.ExpectedImageFilename);
            actual.Save(actualFilename, ImageFormat.Png);

            // load the expected image
            var expected = GetPreviewLayoutTests.LoadImageResource(data.ExpectedImageFilename);

            // save the actual image so we can pick it up as a build artifact
            var expectedFilename = Path.GetFileNameWithoutExtension(data.ExpectedImageFilename) + "_expected" + Path.GetExtension(data.ExpectedImageFilename);
            expected.Save(expectedFilename, ImageFormat.Png);

            // compare the images
            var screens = System.Windows.Forms.Screen.AllScreens;
            AssertImagesEqual(expected, actual);
        }

        private static Bitmap LoadImageResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(assembly.FullName ?? throw new InvalidOperationException());
            var resourceName = $"{assemblyName.Name}.{filename.Replace("/", ".")}";
            var resourceNames = assembly.GetManifestResourceNames();
            if (!resourceNames.Contains(resourceName))
            {
                throw new InvalidOperationException($"Embedded resource '{resourceName}' does not exist.");
            }

            var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException();
            var image = (Bitmap)Image.FromStream(stream);
            return image;
        }

        /// <summary>
        /// Naive / brute force image comparison - we can optimise this later :-)
        /// </summary>
        private static void AssertImagesEqual(Bitmap expected, Bitmap actual)
        {
            Assert.AreEqual(
                expected.Width,
                actual.Width,
                $"expected width: {expected.Width}, actual width: {actual.Width}");
            Assert.AreEqual(
                expected.Height,
                actual.Height,
                $"expected height: {expected.Height}, actual height: {actual.Height}");
            for (var y = 0; y < expected.Height; y++)
            {
                for (var x = 0; x < expected.Width; x++)
                {
                    var expectedPixel = expected.GetPixel(x, y);
                    var actualPixel = actual.GetPixel(x, y);

                    // allow a small tolerance for rounding differences in gdi
                    Assert.IsTrue(
                        (Math.Abs(expectedPixel.A - actualPixel.A) <= 1) &&
                        (Math.Abs(expectedPixel.R - actualPixel.R) <= 1) &&
                        (Math.Abs(expectedPixel.G - actualPixel.G) <= 1) &&
                        (Math.Abs(expectedPixel.B - actualPixel.B) <= 1),
                        $"images differ at pixel ({x}, {y}) - expected: {expectedPixel}, actual: {actualPixel}");
                }
            }
        }
    }
}
