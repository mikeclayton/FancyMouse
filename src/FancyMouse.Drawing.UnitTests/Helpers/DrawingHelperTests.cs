using System.Drawing;
using System.Drawing.Imaging;

using FancyMouse.Drawing.Capture;
using FancyMouse.Drawing.Helpers;
using FancyMouse.Models.Display;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Styles;

namespace FancyMouse.Drawing.UnitTests.Helpers;

public static class DrawingHelperTests
{
    [TestClass]
    public sealed class GetPreviewLayoutTests
    {
        public sealed class TestCase
        {
            public TestCase(PreviewStyle previewStyle, DisplayInfo displayInfo, ScreenInfo activatedScreen, PointInfo activatedLocation, string desktopImageFilename, string expectedImageFilename)
            {
                this.PreviewStyle = previewStyle;
                this.DisplayInfo = displayInfo;
                this.ActivatedScreen = activatedScreen;
                this.ActivatedLocation = activatedLocation;
                this.DesktopImageFilename = desktopImageFilename;
                this.ExpectedImageFilename = expectedImageFilename;
            }

            public PreviewStyle PreviewStyle { get; }

            public DisplayInfo DisplayInfo { get; }

            public ScreenInfo ActivatedScreen { get; }

            public PointInfo ActivatedLocation { get; }

            public string DesktopImageFilename { get; }

            public string ExpectedImageFilename { get; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            // an explicit style rather than the app's "Bezelled" preset so the reference images
            // don't silently depend on the preset's values - and colors like "SystemColors.Highlight"
            // can change between versions of windows, so we use hard-coded colors throughout to
            // reduce false test failures
            var systemHighlight = Color.FromArgb(0, 120, 215);
            var previewStyle = new PreviewStyle(
                canvasSize: new(
                    width: 1600,
                    height: 1200
                ),
                canvasStyle: new(
                    marginStyle: MarginStyle.Empty,
                    borderStyle: new(
                        color: systemHighlight,
                        all: 6,
                        depth: 0
                    ),
                    paddingStyle: new(
                        all: 4
                    ),
                    backgroundStyle: new(
                        color1: Color.FromArgb(0xFF, 0x0D, 0x57, 0xD2),
                        color2: Color.FromArgb(0xFF, 0x03, 0x44, 0xC0)
                    )
                ),
                screenStyle: new(
                    marginStyle: new(
                        all: 4
                    ),
                    borderStyle: new(
                        color: Color.FromArgb(0xFF, 0x22, 0x22, 0x22),
                        all: 15,
                        depth: 3
                    ),
                    paddingStyle: PaddingStyle.Empty,
                    backgroundStyle: new(
                        color1: Color.MidnightBlue,
                        color2: Color.MidnightBlue
                    )
                ),
                extraColors: new[]
                {
                    Color.Red,
                    Color.Blue,
                    Color.Green,
                });

            // 4-grid
            var displayInfo = new DisplayInfo(
                devices: new DeviceInfo[]
                {
                    new(
                        hostname: "localhost",
                        localhost: true,
                        screens: new List<ScreenInfo>
                        {
                            new(
                                handle: 0,
                                primary: true,
                                displayArea: new(0, 0, 500, 500),
                                workingArea: new(0, 0, 500, 500)),
                            new(
                                handle: 0,
                                primary: false,
                                displayArea: new(500, 0, 500, 500),
                                workingArea: new(500, 0, 500, 500)),
                            new(
                                handle: 0,
                                primary: false,
                                displayArea: new(500, 500, 500, 500),
                                workingArea: new(500, 500, 500, 500)),
                            new(
                                handle: 0,
                                primary: false,
                                displayArea: new(0, 500, 500, 500),
                                workingArea: new(0, 500, 500, 500)),
                        }
                    ),
                });
            yield return new object[]
            {
                new TestCase(
                    previewStyle: previewStyle,
                    displayInfo: displayInfo,
                    activatedScreen: displayInfo.Devices[0].Screens[0],
                    activatedLocation: new(x: 50, y: 50),
                    desktopImageFilename: "_test-4grid-desktop.png",
                    expectedImageFilename: "_test-4grid-expected.png"),
            };

            // win 11
            displayInfo = new(
                devices: new DeviceInfo[]
                {
                    new(
                        hostname: "localhost",
                        localhost: true,
                        screens: new List<ScreenInfo>
                        {
                            new(
                                handle: 0,
                                primary: true,
                                displayArea: new(5120, 349, 1920, 1080),
                                workingArea: new(5120, 349, 1920, 1080)),
                            new(
                                handle: 0,
                                primary: false,
                                displayArea: new(0, 0, 5120, 1440),
                                workingArea: new(0, 0, 5120, 1440)),
                        }
                    ),
                }
            );
            yield return new object[]
            {
                new TestCase(
                    previewStyle: previewStyle,
                    displayInfo: displayInfo,
                    activatedScreen: displayInfo.Devices[0].Screens[0],
                    activatedLocation: new(x: 50, y: 50),
                    desktopImageFilename: "_test-win11-desktop.png",
                    expectedImageFilename: "_test-win11-expected.png"),
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases))]
        public async Task RunTestCases(TestCase data)
        {
            // load the fake desktop image
            using var desktopImage = GetPreviewLayoutTests.LoadImageResource(data.DesktopImageFilename);

            var previewLayout = LayoutHelper.GetPreviewLayout(
                previewStyle: data.PreviewStyle,
                displayInfo: data.DisplayInfo,
                maximumSize: data.ActivatedScreen.DisplayArea.Size);

            // draw the preview image - the combined (border+background+bezels/screenshots)
            // render, since the expected golden images were captured against that. Production
            // code no longer flattens these into a single bitmap itself (screenshots/bezels
            // are laid out as native XAML elements by PreviewPane - see its remarks), so this
            // test uses the same shared PreviewHelper.DrawPreviewAsync the SettingsUI style
            // preview does, for comparison purposes only - clamped to the activated screen,
            // matching the physical bound a real on-screen preview would be shown under.
            using var actual = await PreviewHelper.DrawPreviewAsync(
                data.PreviewStyle,
                new StaticScreenshotCaptureProvider(desktopImage),
                data.DisplayInfo,
                maximumSize: data.ActivatedScreen.DisplayArea.Size,
                TestContext.CancellationToken);

            var currentDirectory = Environment.CurrentDirectory;

            // save the actual image so we can pick it up as a build artifact
            var actualFilename = Path.Combine(
                currentDirectory,
                Path.GetFileNameWithoutExtension(data.ExpectedImageFilename) + "_actual" + Path.GetExtension(data.ExpectedImageFilename));
            actual.Save(actualFilename, ImageFormat.Png);

            // load the expected image
            var expected = GetPreviewLayoutTests.LoadImageResource(data.ExpectedImageFilename);

            // save the actual image so we can pick it up as a build artifact
            var expectedFilename = Path.Combine(
                currentDirectory,
                Path.GetFileNameWithoutExtension(data.ExpectedImageFilename) + "_expected" + Path.GetExtension(data.ExpectedImageFilename));
            expected.Save(expectedFilename, ImageFormat.Png);

            // compare the images
            AssertImagesEqual(expected, actual);
        }

        private static Bitmap LoadImageResource(string filename)
        {
            var referenceType = typeof(DrawingHelperTests);
            var resourceAssembly = referenceType.Assembly;

            var resourcePrefix = referenceType.Namespace;
            var resourceName = $"{resourcePrefix}.{filename.Replace("/", ".")}";
            var resourceNames = resourceAssembly.GetManifestResourceNames();
            if (!resourceNames.Contains(resourceName))
            {
                var message = string.Join(
                    Environment.NewLine,
                    new List<string>([
                        $"Embedded resource '{resourceName}' does not exist.",
                        string.Empty,
                        "Valid resource names are:",
                        string.Empty])
                    .Concat(resourceNames));
                throw new InvalidOperationException(message);
            }

            var stream = resourceAssembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException();
            var image = (Bitmap)Image.FromStream(stream);
            return image;
        }

        /// <summary>
        /// Naive / brute force image comparison - we can optimize this later :-)
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

                    // allow a 1-level-per-channel tolerance for residual GDI+ rasterization rounding.
                    Assert.IsTrue(
                        (Math.Abs(expectedPixel.A - actualPixel.A) <= 1) &&
                        (Math.Abs(expectedPixel.R - actualPixel.R) <= 1) &&
                        (Math.Abs(expectedPixel.G - actualPixel.G) <= 1) &&
                        (Math.Abs(expectedPixel.B - actualPixel.B) <= 1),
                        $"images differ at pixel ({x}, {y}) - expected: {expectedPixel}, actual: {actualPixel}");
                }
            }
        }

        public TestContext TestContext { get; set; }
    }
}
