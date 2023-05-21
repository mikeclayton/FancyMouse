﻿using FancyMouse.Helpers;
using FancyMouse.Models.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FancyMouse.UnitTests.Helpers;

[TestClass]
public static class MouseHelperTests
{
    [TestClass]
    public sealed class GetJumpLocationTests
    {
        public sealed class TestCase
        {
            public TestCase(PointInfo previewLocation, SizeInfo previewSize,  RectangleInfo desktopBounds, PointInfo expectedResult)
            {
                this.PreviewLocation = previewLocation;
                this.PreviewSize = previewSize;
                this.DesktopBounds = desktopBounds;
                this.ExpectedResult = expectedResult;
            }

            public PointInfo PreviewLocation { get; set; }

            public SizeInfo PreviewSize { get; set; }

            public RectangleInfo DesktopBounds { get; set; }

            public PointInfo ExpectedResult { get; set; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            // screen corners and midpoint with a zero origin
            yield return new[] { new TestCase(new(0, 0), new(160, 120), new(0, 0, 1600, 1200), new(0, 0)) };
            yield return new[] { new TestCase(new(160, 0), new(160, 120), new(0, 0, 1600, 1200), new(1600, 0)) };
            yield return new[] { new TestCase(new(0, 120), new(160, 120), new(0, 0, 1600, 1200), new(0, 1200)) };
            yield return new[] { new TestCase(new(160, 120), new(160, 120), new(0, 0, 1600, 1200), new(1600, 1200)) };
            yield return new[] { new TestCase(new(80, 60), new(160, 120), new(0, 0, 1600, 1200), new(800, 600)) };

            // screen corners and midpoint with a positive origin
            yield return new[] { new TestCase(new(0, 0), new(160, 120), new(1000, 1000, 1600, 1200), new(1000, 1000)) };
            yield return new[] { new TestCase(new(160, 0), new(160, 120), new(1000, 1000, 1600, 1200), new(2600, 1000)) };
            yield return new[] { new TestCase(new(0, 120), new(160, 120), new(1000, 1000, 1600, 1200), new(1000, 2200)) };
            yield return new[] { new TestCase(new(160, 120), new(160, 120), new(1000, 1000, 1600, 1200), new(2600, 2200)) };
            yield return new[] { new TestCase(new(80, 60), new(160, 120), new(1000, 1000, 1600, 1200), new(1800, 1600)) };

            // screen corners and midpoint with a negative origin
            yield return new[] { new TestCase(new(0, 0), new(160, 120), new(-1000, -1000, 1600, 1200), new(-1000, -1000)) };
            yield return new[] { new TestCase(new(160, 0), new(160, 120), new(-1000, -1000, 1600, 1200), new(600, -1000)) };
            yield return new[] { new TestCase(new(0, 120), new(160, 120), new(-1000, -1000, 1600, 1200), new(-1000, 200)) };
            yield return new[] { new TestCase(new(160, 120), new(160, 120), new(-1000, -1000, 1600, 1200), new(600, 200)) };
            yield return new[] { new TestCase(new(80, 60), new(160, 120), new(-1000, -1000, 1600, 1200), new(-200, -400)) };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void RunTestCases(TestCase data)
        {
            var actual = MouseHelper.GetJumpLocation(
                data.PreviewLocation,
                data.PreviewSize,
                data.DesktopBounds);
            var expected = data.ExpectedResult;
            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Y, actual.Y);
        }
    }
}
