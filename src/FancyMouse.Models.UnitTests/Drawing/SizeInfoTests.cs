using FancyMouse.Models.Drawing;

namespace FancyMouse.Models.UnitTests.Drawing;

public static class SizeInfoTests
{
    [TestClass]
    public sealed class ScaleToFitTests
    {
        public sealed class TestCase
        {
            public TestCase(SizeInfo source, SizeInfo bounds, SizeInfo expectedResult, decimal scalingRatio)
            {
                this.Source = source;
                this.Bounds = bounds;
                this.ExpectedResult = expectedResult;
                this.ScalingRatio = scalingRatio;
            }

            public SizeInfo Source { get; }

            public SizeInfo Bounds { get; }

            public SizeInfo ExpectedResult { get; }

            public decimal ScalingRatio { get; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            // identity tests
            yield return new object[] { new TestCase(new(512, 384), new(512, 384), new(512, 384), 1), };
            yield return new object[] { new TestCase(new(1024, 768), new(1024, 768), new(1024, 768), 1), };

            // general tests
            yield return new object[] { new TestCase(new(512, 384), new(2048, 1536), new(2048, 1536), 4), };
            yield return new object[] { new TestCase(new(2048, 1536), new(1024, 768), new(1024, 768), 0.5m), };

            // scale to fit width
            yield return new object[] { new TestCase(new(512, 384), new(2048, 3072), new(2048, 1536), 4), };

            // scale to fit height
            yield return new object[] { new TestCase(new(512, 384), new(4096, 1536), new(2048, 1536), 4), };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases))]
        public void RunTestCases(TestCase data)
        {
            var actual = data.Source.ScaleToFit(data.Bounds, out var scalingRatio);
            var expected = data.ExpectedResult;
            Assert.AreEqual(expected.Width, actual.Width);
            Assert.AreEqual(expected.Height, actual.Height);
            Assert.AreEqual(scalingRatio, data.ScalingRatio);
        }

        [TestMethod]
        [DataRow(0, 384)]
        [DataRow(512, 0)]
        [DataRow(0, 0)]
        public void ZeroSizedSourceThrows(int width, int height)
        {
            var source = new SizeInfo(width, height);
            Assert.ThrowsExactly<InvalidOperationException>(() => source.ScaleToFit(new SizeInfo(1024, 768), out _));
        }
    }

    [TestClass]
    public sealed class ScaleToFitRatioTests
    {
        public sealed class TestCase
        {
            public TestCase(SizeInfo obj, SizeInfo bounds, decimal expectedResult)
            {
                this.Obj = obj;
                this.Bounds = bounds;
                this.ExpectedResult = expectedResult;
            }

            public SizeInfo Obj { get; }

            public SizeInfo Bounds { get; }

            public decimal ExpectedResult { get; }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            // identity tests
            yield return new object[] { new TestCase(new(512, 384), new(512, 384), 1), };
            yield return new object[] { new TestCase(new(1024, 768), new(1024, 768), 1), };

            // general tests
            yield return new object[] { new TestCase(new(512, 384), new(2048, 1536), 4), };
            yield return new object[] { new TestCase(new(2048, 1536), new(1024, 768), 0.5M), };

            // scale to fit width
            yield return new object[] { new TestCase(new(512, 384), new(2048, 3072), 4), };

            // scale to fit height
            yield return new object[] { new TestCase(new(512, 384), new(4096, 1536), 4), };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases))]
        public void RunTestCases(TestCase data)
        {
            var actual = data.Obj.ScaleToFitRatio(data.Bounds);
            var expected = data.ExpectedResult;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(0, 384)]
        [DataRow(512, 0)]
        [DataRow(0, 0)]
        public void ZeroSizedSourceThrows(int width, int height)
        {
            var source = new SizeInfo(width, height);
            Assert.ThrowsExactly<InvalidOperationException>(() => source.ScaleToFitRatio(new SizeInfo(1024, 768)));
        }

        [TestMethod]
        [DataRow(0, 768)]
        [DataRow(1024, 0)]
        [DataRow(0, 0)]
        public void ZeroSizedBoundsThrows(int width, int height)
        {
            var source = new SizeInfo(512, 384);
            Assert.ThrowsExactly<ArgumentException>(() => source.ScaleToFitRatio(new SizeInfo(width, height)));
        }
    }
}
