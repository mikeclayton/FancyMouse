using NUnit.Framework;
using System.Drawing;

namespace FancyMouse.Lib.Tests;

public static class LayoutHelperTests
{

    #region General Helpers

    public static class CombineBoundsTests
    {

        private static IEnumerable<(IEnumerable<Rectangle> Bounds, Rectangle ExpectedResult)> GetTestCases()
        {
            // empty list
            yield return (
                new List<Rectangle> { },
                Rectangle.Empty
            );
            // empty bounds
            yield return (
                new List<Rectangle> {
                    Rectangle.Empty
                },
                Rectangle.Empty
            );
            // single region
            //
            // +---+
            // | 0 |
            // +---+
            yield return (
                new List<Rectangle> {
                    new Rectangle(100,100,100,100)
                },
                new Rectangle(100, 100, 100, 100)
            );
            // multi-monitor desktop
            //
            // +----------------+
            // |                |
            // |       1        +-------+
            // |                |   0   |
            // +----------------+-------+
            yield return (
                new List<Rectangle> {
                    new Rectangle(5120, 0, 1920, 1080),
                    new Rectangle(0, 0, 5120, 1440),
                },
                new Rectangle(0, 0, 7040, 1440)
            );
            // multi-monitor desktop
            //
            // note - windows puts the *primary* monitor at the origin (0,0),
            // so screens positioned *above* or *left* will have negative coordinates
            //
            // +-------+
            // |   0   |
            // +-------+--------+
            // |                |
            // |       1        |
            // |                |
            // +----------------+
            yield return (
                new List<Rectangle> {
                    new Rectangle(0, -1000, 1920, 1080),
                    new Rectangle(0, 0, 5120, 1440),
                },
                new Rectangle(0, -1000, 5120, 2440)
            );
            // multi-monitor desktop
            //
            // note - windows puts the *primary* monitor at the origin (0,0),
            // so screens positioned *above* or *left* will have negative coordinates
            //
            // +-------+----------------+
            // |   0   |                |
            // +-------+       1        |
            //         |                |
            //         +----------------+
            yield return (
                new List<Rectangle> {
                    new Rectangle(-1920, 0, 1920, 1080),
                    new Rectangle(0, 0, 5120, 1440),
                },
                new Rectangle(-1920, 0, 7040, 1440)
            );
            // non-contiguous regions
            //
            // +---+
            // | 0 |    +-------+
            // +---+    |       |
            //          |   1   |
            //          |       |
            //          +-------+
            yield return (
                new List<Rectangle> {
                    new Rectangle(0, 0, 100, 100),
                    new Rectangle(200, 150, 200, 200),
                },
                new Rectangle(0, 0, 400, 350)
            );
        }

        [TestCaseSource(nameof(GetTestCases))]
        public static void RunTestCases((IEnumerable<Rectangle> Bounds, Rectangle ExpectedResult) data)
        {
            var actual = LayoutHelper.CombineBounds(data.Bounds);
            var expected = data.ExpectedResult;
            Assert.AreEqual(expected, actual);
        }

    }

    public static class ScaleToFitTests
    {

        private static IEnumerable<(Size Obj, Size Bounds, Size ExpectedResult)> GetTestCases()
        {
            // identity tests
            yield return (new Size(0, 0), new Size(0, 0), new Size(0, 0));
            yield return (new Size(512, 384), new Size(512, 384), new Size(512, 384));
            yield return (new Size(1024, 768), new Size(1024, 768), new Size(1024, 768));
            // integer scaling factor tests
            yield return (new Size(512, 384), new Size(2048, 1536), new Size(2048, 1536));
            yield return (new Size(2048, 1536), new Size(1024, 768), new Size(1024, 768));
            // scale to fit width
            yield return (new Size(512, 384), new Size(2048, 3072), new Size(2048, 1536));
            // scale to fit height
            yield return (new Size(512, 384), new Size(4096, 1536), new Size(2048, 1536));
        }

        [TestCaseSource(nameof(GetTestCases))]
        public static void RunTestCases((Size Obj, Size Bounds, Size ExpectedResult) data)
        {
            var actual = LayoutHelper.ScaleToFit(data.Obj,data.Bounds);
            var expected = data.ExpectedResult;
            Assert.AreEqual(expected, actual);
        }

    }

    public static class CenterTests
    {

        private static IEnumerable<(Size Obj, Point Midpoint, Point ExpectedResult)> GetTestCases()
        {
            // zero-sized object should centre exactly on the midpoint
            yield return (new Size(0, 0), new Point(0, 0), new Point(0, 0));
            // odd-sized objects should centre above/left of the midpoint
            yield return (new Size(1, 1), new Point(1, 1), new Point(0, 0));
            yield return (new Size(1, 1), new Point(5, 5), new Point(4, 4));
            // even-sized objects should centre exactly on the midpoint
            yield return (new Size(2, 2), new Point(1, 1), new Point(0, 0));
            yield return (new Size(2, 2), new Point(5, 5), new Point(4, 4));
            yield return (new Size(800, 600), new Point(1000, 1000), new Point(600, 700));
            // negative result position
            yield return (new Size(1000, 1200), new Point(300, 300), new Point(-200, -300));
        }

        [TestCaseSource(nameof(GetTestCases))]
        public static void RunTestCases((Size Obj, Point Midpoint, Point ExpectedResult) data)
        {
            var actual = LayoutHelper.Center(data.Obj, data.Midpoint);
            var expected = data.ExpectedResult;
            Assert.AreEqual(expected, actual);
        }

    }

    public static class MoveInsideTests
    {

        private static IEnumerable<(Rectangle Obj, Rectangle Bounds, Rectangle ExpectedResult)> GetTestCases()
        {
            // already inside - obj fills bounds
            yield return (new Rectangle(0, 0, 100, 100), new Rectangle(0, 0, 100, 100), new Rectangle(0, 0, 100, 100));
            // already inside - obj exactly in each corner
            yield return (new Rectangle(0, 0, 100, 100), new Rectangle(0, 0, 200, 200), new Rectangle(0, 0, 100, 100));
            yield return (new Rectangle(100, 0, 100, 100), new Rectangle(0, 0, 200, 200), new Rectangle(100, 0, 100, 100));
            yield return (new Rectangle(0, 100, 100, 100), new Rectangle(0, 0, 200, 200), new Rectangle(0, 100, 100, 100));
            yield return (new Rectangle(100, 100, 100, 100), new Rectangle(0, 0, 200, 200), new Rectangle(100, 100, 100, 100));
            // move inside - obj outside each corner
            yield return (new Rectangle(-50, -50, 100, 100), new Rectangle(0, 0, 200, 200), new Rectangle(0, 0, 100, 100));
            yield return (new Rectangle(250, -50, 100, 100), new Rectangle(0, 0, 200, 200), new Rectangle(100, 0, 100, 100));
            yield return (new Rectangle(-50, 250, 100, 100), new Rectangle(0, 0, 200, 200), new Rectangle(0, 100, 100, 100));
            yield return (new Rectangle(150, 150, 100, 100), new Rectangle(0, 0, 200, 200), new Rectangle(100, 100, 100, 100));
        }

        [TestCaseSource(nameof(GetTestCases))]
        public static void RunTestCases((Rectangle Obj, Rectangle Bounds, Rectangle ExpectedResult) data)
        {
            var actual = LayoutHelper.MoveInside(data.Obj, data.Bounds);
            var expected = data.ExpectedResult;
            Assert.AreEqual(expected, actual);
        }

    }

    public static class BetweenTests
    {

        private static IEnumerable<(int Min, int Value, int Max, int ExpectedResult)> GetTestCases()
        {
            yield return (0, 0, 0, 0);
            yield return (0, -100, 100, 0);
            yield return (0, 0, 100, 0);
            yield return (0, 50, 100, 50);
            yield return (0, 100, 100, 100);
            yield return (0, 200, 100, 100);
        }

        [TestCaseSource(nameof(GetTestCases))]
        public static void RunTestCases((int Min, int Value, int Max, int Expected) data)
        {
            var actual = LayoutHelper.Between(data.Min, data.Value, data.Max);
            var expected = data.Expected;
            Assert.AreEqual(expected, actual);
        }

    }

    #endregion

    public static class GetPreviewPositionTests
    {
    }

}