using System.Drawing;
using System.Text.Json;
using FancyMouse.Helpers;
using FancyMouse.Models.Drawing;
using FancyMouse.Models.Layout;
using FancyMouse.Models.Styles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FancyMouse.UnitTests.Helpers;

[TestClass]
public static class LayoutHelperTests
{
    /*
    [TestClass]
    public sealed class OldLayoutTests
    {

        public static IEnumerable<object[]> GetTestCases()
        {
            // primary monitor not topmost / leftmost - if there are screens
            // that are further left or higher than the primary monitor
            // they'll have negative coordinates which has caused some
            // issues with calculations in the past. this test will make
            // sure we handle negative coordinates gracefully
            //
            // +-------+
            // |   0   +----------------+
            // +-------+                |
            //         |       1        |
            //         |                |
            //         +----------------+
            layoutConfig = new LayoutConfig(
                virtualScreenBounds: new(-1920, -472, 7040, 1912),
                screens: new List<ScreenInfo>
                {
                    new(HMONITOR.Null, false, new(-1920, -472, 1920, 1080), new(-1920, -472, 1920, 1080)),
                    new(HMONITOR.Null, false, new(0, 0, 5120, 1440), new(0, 0, 5120, 1440)),
                },
                activatedLocation: new(-960, -236),
                activatedScreenIndex: 0,
                activatedScreenNumber: 1,
                maximumFormSize: new(1600, 1200),
                formPadding: new(5, 5, 5, 5),
                previewPadding: new(0, 0, 0, 0));
            layoutInfo = new LayoutInfo(
                layoutConfig: layoutConfig,
                formBounds: new(
                    -1760,
                    -456.91477M, // -236 - (((decimal)(1600-10) / 7040 * 1912) + 10) / 2
                    1600,
                    441.829545M // ((decimal)(1600-10) / 7040 * 1912) + 10
                ),
                previewBounds: new(0, 0, 1590, 431.829545M),
                screenBounds: new List<RectangleInfo>
                {
                    new(0, 0, 433.63636M, 243.92045M),
                    new(433.63636M, 106.602270M, 1156.36363M, 325.22727M),
                },
                activatedScreenBounds: new(-1920, -472, 1920, 1080));
            yield return new object[] { new TestCase(layoutConfig, layoutInfo) };

            // check we handle rounding errors in scaling the preview form
            // that might make the form *larger* than the current screen -
            // e.g. a desktop 7168 x 1440 scaled to a screen 1024 x 768
            // with a 5px form padding border:
            //
            // ((decimal)1014 / 7168) * 7168 = 1014.0000000000000000000000002
            //
            // +----------------+
            // |                |
            // |       1        +-------+
            // |                |   0   |
            // +----------------+-------+
            layoutConfig = new LayoutConfig(
                virtualScreenBounds: new(0, 0, 7168, 1440),
                screens: new List<ScreenInfo>
                {
                    new(HMONITOR.Null, false, new(6144, 0, 1024, 768), new(6144, 0, 1024, 768)),
                    new(HMONITOR.Null, false, new(0, 0, 6144, 1440), new(0, 0, 6144, 1440)),
                },
                activatedLocation: new(6656, 384),
                activatedScreenIndex: 0,
                activatedScreenNumber: 1,
                maximumFormSize: new(1600, 1200),
                formPadding: new(5, 5, 5, 5),
                previewPadding: new(0, 0, 0, 0));
            layoutInfo = new LayoutInfo(
                layoutConfig: layoutConfig,
                formBounds: new(6144, 277.14732M, 1024, 213.70535M),
                previewBounds: new(0, 0, 1014, 203.70535M),
                screenBounds: new List<RectangleInfo>
                {
                    new(869.14285M, 0, 144.85714M, 108.642857M),
                    new(0, 0, 869.142857M, 203.705357M),
                },
                activatedScreenBounds: new(6144, 0, 1024, 768));
            yield return new object[] { new TestCase(layoutConfig, layoutInfo) };

            // check we handle rounding errors in scaling the preview form
            // that might make the form a pixel *smaller* than the current screen -
            // e.g. a desktop 7168 x 1440 scaled to a screen 1024 x 768
            // with a 5px form padding border:
            //
            // ((decimal)1280 / 7424) * 7424 = 1279.9999999999999999999999999
            //
            // +----------------+
            // |                |
            // |       1        +-------+
            // |                |   0   |
            // +----------------+-------+
            layoutConfig = new LayoutConfig(
                virtualScreenBounds: new(0, 0, 7424, 1440),
                screens: new List<ScreenInfo>
                {
                    new(HMONITOR.Null, false, new(6144, 0, 1280, 768), new(6144, 0, 1280, 768)),
                    new(HMONITOR.Null, false, new(0, 0, 6144, 1440), new(0, 0, 6144, 1440)),
                },
                activatedLocation: new(6784, 384),
                activatedScreenIndex: 0,
                activatedScreenNumber: 1,
                maximumFormSize: new(1600, 1200),
                formPadding: new(5, 5, 5, 5),
                previewPadding: new(0, 0, 0, 0));
            layoutInfo = new LayoutInfo(
                layoutConfig: layoutConfig,
                formBounds: new(
                    6144,
                    255.83189M, // (768 - (((decimal)(1280-10) / 7424 * 1440) + 10)) / 2
                    1280,
                    256.33620M // ((decimal)(1280 - 10) / 7424 * 1440) + 10
                ),
                previewBounds: new(0, 0, 1270, 246.33620M),
                screenBounds: new List<RectangleInfo>
                {
                    new(1051.03448M, 0, 218.96551M, 131.37931M),
                    new(0, 0M, 1051.03448M, 246.33620M),
                },
                activatedScreenBounds: new(6144, 0, 1280, 768));
            yield return new object[] { new TestCase(layoutConfig, layoutInfo) };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void RunTestCases(TestCase data)
        {
            // note - even if values are within 0.0001M of each other they could
            // still round to different values - e.g.
            // (int)1279.999999999999 -> 1279
            // vs
            // (int)1280.000000000000 -> 1280
            // so we'll compare the raw values, *and* convert to an int-based
            // Rectangle to compare rounded values
            var actual = LayoutHelper.GetPreviewLayout(data.PreviewStyle, data.Screens, data.ActivatedLocation);
            var expected = data.ExpectedResult;
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
            Assert.AreEqual(expected.FormBounds.X, actual.FormBounds.X, 0.00001M, "FormBounds.X");
            Assert.AreEqual(expected.FormBounds.Y, actual.FormBounds.Y, 0.00001M, "FormBounds.Y");
            Assert.AreEqual(expected.FormBounds.Width, actual.FormBounds.Width, 0.00001M, "FormBounds.Width");
            Assert.AreEqual(expected.FormBounds.Height, actual.FormBounds.Height, 0.00001M, "FormBounds.Height");
            Assert.AreEqual(expected.FormBounds.ToRectangle(), actual.FormBounds.ToRectangle(), "FormBounds.ToRectangle");
            Assert.AreEqual(expected.PreviewBounds.X, actual.PreviewBounds.X, 0.00001M, "PreviewBounds.X");
            Assert.AreEqual(expected.PreviewBounds.Y, actual.PreviewBounds.Y, 0.00001M, "PreviewBounds.Y");
            Assert.AreEqual(expected.PreviewBounds.Width, actual.PreviewBounds.Width, 0.00001M, "PreviewBounds.Width");
            Assert.AreEqual(expected.PreviewBounds.Height, actual.PreviewBounds.Height, 0.00001M, "PreviewBounds.Height");
            Assert.AreEqual(expected.PreviewBounds.ToRectangle(), actual.PreviewBounds.ToRectangle(), "PreviewBounds.ToRectangle");
            Assert.AreEqual(expected.ScreenBounds.Count, actual.ScreenBounds.Count, "ScreenBounds.Count");
            for (var i = 0; i < expected.ScreenBounds.Count; i++)
            {
                Assert.AreEqual(expected.ScreenBounds[i].X, actual.ScreenBounds[i].X, 0.00001M, $"ScreenBounds[{i}].X");
                Assert.AreEqual(expected.ScreenBounds[i].Y, actual.ScreenBounds[i].Y, 0.00001M, $"ScreenBounds[{i}].Y");
                Assert.AreEqual(expected.ScreenBounds[i].Width, actual.ScreenBounds[i].Width, 0.00001M, $"ScreenBounds[{i}].Width");
                Assert.AreEqual(expected.ScreenBounds[i].Height, actual.ScreenBounds[i].Height, 0.00001M, $"ScreenBounds[{i}].Height");
                Assert.AreEqual(expected.ScreenBounds[i].ToRectangle(), actual.ScreenBounds[i].ToRectangle(), "ActivatedScreen.ToRectangle");
            }

            Assert.AreEqual(expected.ActivatedScreenBounds.X, actual.ActivatedScreenBounds.X, "ActivatedScreen.X");
            Assert.AreEqual(expected.ActivatedScreenBounds.Y, actual.ActivatedScreenBounds.Y, "ActivatedScreen.Y");
            Assert.AreEqual(expected.ActivatedScreenBounds.Width, actual.ActivatedScreenBounds.Width, "ActivatedScreen.Width");
            Assert.AreEqual(expected.ActivatedScreenBounds.Height, actual.ActivatedScreenBounds.Height, "ActivatedScreen.Height");
            Assert.AreEqual(expected.ActivatedScreenBounds.ToRectangle(), actual.ActivatedScreenBounds.ToRectangle(), "ActivatedScreen.ToRectangle");
        }
    }
    */

    [TestClass]
    public sealed class GetPreviewLayoutTests
    {
        public sealed class TestCase
        {
            public TestCase(PreviewStyle previewStyle, List<RectangleInfo> screens, PointInfo activatedLocation, PreviewLayout expectedResult)
            {
                this.PreviewStyle = previewStyle;
                this.Screens = screens;
                this.ActivatedLocation = activatedLocation;
                this.ExpectedResult = expectedResult;
            }

            public PreviewStyle PreviewStyle { get; set; }

            public List<RectangleInfo> Screens { get; set; }

            public PointInfo ActivatedLocation { get; set; }

            public PreviewLayout ExpectedResult { get; set; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            // happy path - 50% scaling, *has* preview borders but *no* screenshot borders
            //
            // +----------------+
            // |                |
            // |       0        |
            // |                |
            // +----------------+
            var previewStyle = new PreviewStyle(
                canvasSize: new(
                    width: 524,
                    height: 396
                ),
                canvasStyle: new(
                    marginStyle: MarginStyle.Empty,
                    borderStyle: new(
                        color: SystemColors.Highlight,
                        all: 5,
                        depth: 3),
                    paddingStyle: new(
                        all: 1),
                    backgroundStyle: new(
                        color1: Color.FromArgb(13, 87, 210), // light blue
                        color2: Color.FromArgb(3, 68, 192) // darker blue
                    )
                ),
                screenshotStyle: BoxStyle.Empty);
            var screens = new List<RectangleInfo>
            {
                new(0, 0, 1024, 768),
            };
            var activatedLocation = new PointInfo(512, 384);
            var previewLayout = new PreviewLayout(
                virtualScreen: new(0, 0, 1024, 768),
                screens: screens,
                activatedScreenIndex: 0,
                formBounds: new(250, 186, 524, 396),
                previewStyle: previewStyle,
                previewBounds: new(
                    outerBounds: new(0, 0, 524, 396),
                    marginBounds: new(0, 0, 524, 396),
                    borderBounds: new(0, 0, 524, 396),
                    paddingBounds: new(5, 5, 514, 386),
                    contentBounds: new(6, 6, 512, 384)
                ),
                screenshotBounds: new()
                {
                    new(
                        outerBounds: new(6, 6, 512, 384),
                        marginBounds: new(6, 6, 512, 384),
                        borderBounds: new(6, 6, 512, 384),
                        paddingBounds: new(6, 6, 512, 384),
                        contentBounds: new(6, 6, 512, 384)
                    ),
                });
            yield return new object[] { new TestCase(previewStyle, screens, activatedLocation, previewLayout) };

            // happy path - 50% scaling, *no* preview borders but *has* screenshot borders
            //
            // +----------------+
            // |                |
            // |       0        |
            // |                |
            // +----------------+
            previewStyle = new PreviewStyle(
                canvasSize: new(
                    width: 512,
                    height: 384
                ),
                canvasStyle: BoxStyle.Empty,
                screenshotStyle: new(
                    marginStyle: new(
                        all: 1),
                    borderStyle: new(
                        color: SystemColors.Highlight,
                        all: 5,
                        depth: 3),
                    paddingStyle: PaddingStyle.Empty,
                    backgroundStyle: new(
                        color1: Color.FromArgb(13, 87, 210), // light blue
                        color2: Color.FromArgb(3, 68, 192) // darker blue
                    )
                ));
            screens = new List<RectangleInfo>
            {
                new(0, 0, 1024, 768),
            };
            activatedLocation = new PointInfo(512, 384);
            previewLayout = new PreviewLayout(
                virtualScreen: new(0, 0, 1024, 768),
                screens: screens,
                activatedScreenIndex: 0,
                formBounds: new(256, 192, 512, 384),
                previewStyle: previewStyle,
                previewBounds: new(
                    outerBounds: new(0, 0, 512, 384),
                    marginBounds: new(0, 0, 512, 384),
                    borderBounds: new(0, 0, 512, 384),
                    paddingBounds: new(0, 0, 512, 384),
                    contentBounds: new(0, 0, 512, 384)
                ),
                screenshotBounds: new()
                {
                    new(
                        outerBounds: new(0, 0, 512, 384),
                        marginBounds: new(0, 0, 512, 384),
                        borderBounds: new(1, 1, 510, 382),
                        paddingBounds: new(6, 6, 500, 372),
                        contentBounds: new(6, 6, 500, 372)
                    ),
                });
            yield return new object[] { new TestCase(previewStyle, screens, activatedLocation, previewLayout) };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void RunTestCases(TestCase data)
        {
            // note - even if values are within 0.0001M of each other they could
            // still round to different values - e.g.
            // (int)1279.999999999999 -> 1279
            // vs
            // (int)1280.000000000000 -> 1280
            // so we'll compare the raw values, *and* convert to an int-based
            // Rectangle to compare rounded values
            var actual = LayoutHelper.GetPreviewLayout(data.PreviewStyle, data.Screens, data.ActivatedLocation);
            var expected = data.ExpectedResult;
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            Assert.AreEqual(
                JsonSerializer.Serialize(expected, options),
                JsonSerializer.Serialize(actual, options));
            /* form bounds */
            /*
            Assert.AreEqual(expected.FormBounds.X, actual.FormBounds.X, 0.00001M, "FormBounds.X");
            Assert.AreEqual(expected.FormBounds.Y, actual.FormBounds.Y, 0.00001M, "FormBounds.Y");
            Assert.AreEqual(expected.FormBounds.Width, actual.FormBounds.Width, 0.00001M, "FormBounds.Width");
            Assert.AreEqual(expected.FormBounds.Height, actual.FormBounds.Height, 0.00001M, "FormBounds.Height");
            Assert.AreEqual(expected.FormBounds.ToRectangle(), actual.FormBounds.ToRectangle(), "FormBounds.ToRectangle");
            */
            /* preview bounds */
            /*
            Assert.AreEqual(expected.PreviewBounds.X, actual.PreviewBounds.X, 0.00001M, "PreviewBounds.X");
            Assert.AreEqual(expected.PreviewBounds.Y, actual.PreviewBounds.Y, 0.00001M, "PreviewBounds.Y");
            Assert.AreEqual(expected.PreviewBounds.Width, actual.PreviewBounds.Width, 0.00001M, "PreviewBounds.Width");
            Assert.AreEqual(expected.PreviewBounds.Height, actual.PreviewBounds.Height, 0.00001M, "PreviewBounds.Height");
            Assert.AreEqual(expected.PreviewBounds.ToRectangle(), actual.PreviewBounds.ToRectangle(), "PreviewBounds.ToRectangle");
            */
            /*
            Assert.AreEqual(expected.ScreenBounds.Count, actual.ScreenBounds.Count, "ScreenBounds.Count");
            for (var i = 0; i < expected.ScreenBounds.Count; i++)
            {
                Assert.AreEqual(expected.ScreenBounds[i].X, actual.ScreenBounds[i].X, 0.00001M, $"ScreenBounds[{i}].X");
                Assert.AreEqual(expected.ScreenBounds[i].Y, actual.ScreenBounds[i].Y, 0.00001M, $"ScreenBounds[{i}].Y");
                Assert.AreEqual(expected.ScreenBounds[i].Width, actual.ScreenBounds[i].Width, 0.00001M, $"ScreenBounds[{i}].Width");
                Assert.AreEqual(expected.ScreenBounds[i].Height, actual.ScreenBounds[i].Height, 0.00001M, $"ScreenBounds[{i}].Height");
                Assert.AreEqual(expected.ScreenBounds[i].ToRectangle(), actual.ScreenBounds[i].ToRectangle(), "ActivatedScreen.ToRectangle");
            }

            Assert.AreEqual(expected.ActivatedScreenBounds.X, actual.ActivatedScreenBounds.X, "ActivatedScreen.X");
            Assert.AreEqual(expected.ActivatedScreenBounds.Y, actual.ActivatedScreenBounds.Y, "ActivatedScreen.Y");
            Assert.AreEqual(expected.ActivatedScreenBounds.Width, actual.ActivatedScreenBounds.Width, "ActivatedScreen.Width");
            Assert.AreEqual(expected.ActivatedScreenBounds.Height, actual.ActivatedScreenBounds.Height, "ActivatedScreen.Height");
            Assert.AreEqual(expected.ActivatedScreenBounds.ToRectangle(), actual.ActivatedScreenBounds.ToRectangle(), "ActivatedScreen.ToRectangle");
            */
        }
    }

    [TestClass]
    public sealed class GetBoxBoundsFromContentBoundsTests
    {
        public sealed class TestCase
        {
            public TestCase(RectangleInfo contentBounds, BoxStyle boxStyle, BoxBounds expectedResult)
            {
                this.ContentBounds = contentBounds;
                this.BoxStyle = boxStyle;
                this.ExpectedResult = expectedResult;
            }

            public RectangleInfo ContentBounds { get; set; }

            public BoxStyle BoxStyle { get; set; }

            public BoxBounds ExpectedResult { get; set; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            yield return new[]
            {
                new TestCase(
                    contentBounds: new(100, 100, 800, 600),
                    boxStyle: new(
                        marginStyle: new(3),
                        borderStyle: new(Color.Red, 5, 0),
                        paddingStyle: new(7),
                        backgroundStyle: BackgroundStyle.Empty),
                    expectedResult: new(
                        outerBounds: new(85, 85, 830, 630),
                        marginBounds: new(85, 85, 830, 630),
                        borderBounds: new(88, 88, 824, 624),
                        paddingBounds: new(93, 93, 814, 614),
                        contentBounds: new(100, 100, 800, 600))),
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void RunTestCases(TestCase data)
        {
            var actual = LayoutHelper.GetBoxBoundsFromContentBounds(data.ContentBounds, data.BoxStyle);
            var expected = data.ExpectedResult;
            Assert.AreEqual(
            JsonSerializer.Serialize(expected),
            JsonSerializer.Serialize(actual));
        }
    }

    [TestClass]
    public sealed class GetBoxBoundsFromOuterBoundsTests
    {
        public sealed class TestCase
        {
            public TestCase(RectangleInfo outerBounds, BoxStyle boxStyle, BoxBounds expectedResult)
            {
                this.OuterBounds = outerBounds;
                this.BoxStyle = boxStyle;
                this.ExpectedResult = expectedResult;
            }

            public RectangleInfo OuterBounds { get; set; }

            public BoxStyle BoxStyle { get; set; }

            public BoxBounds ExpectedResult { get; set; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            yield return new[]
            {
                new TestCase(
                    outerBounds: new(85, 85, 830, 630),
                    boxStyle: new(
                        marginStyle: new(3),
                        borderStyle: new(Color.Red, 5, 0),
                        paddingStyle: new(7),
                        backgroundStyle: BackgroundStyle.Empty),
                    expectedResult: new(
                        outerBounds: new(85, 85, 830, 630),
                        marginBounds: new(85, 85, 830, 630),
                        borderBounds: new(88, 88, 824, 624),
                        paddingBounds: new(93, 93, 814, 614),
                        contentBounds: new(100, 100, 800, 600))),
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void RunTestCases(TestCase data)
        {
            var actual = LayoutHelper.GetBoxBoundsFromOuterBounds(data.OuterBounds, data.BoxStyle);
            var expected = data.ExpectedResult;
            Assert.AreEqual(
                JsonSerializer.Serialize(expected),
                JsonSerializer.Serialize(actual));
        }
    }
}
