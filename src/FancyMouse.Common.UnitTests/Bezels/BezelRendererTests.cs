using System.Drawing;

using FancyMouse.Common.Bezels;
using FancyMouse.Models.Styles;

namespace FancyMouse.Common.UnitTests.Bezels;

public static class BezelRendererTests
{
    [TestClass]
    public sealed class ClampDepthTests
    {
        public sealed class TestCase
        {
            public TestCase(string testName, BorderStyle borderStyle, BorderStyle expectedResult)
            {
                this.TestName = testName;
                this.BorderStyle = borderStyle;
                this.ExpectedResult = expectedResult;
            }

            public string TestName { get; }

            public BorderStyle BorderStyle { get; }

            public BorderStyle ExpectedResult { get; }

            public override string ToString() => this.TestName;
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            // depth well within half the thickness - unchanged
            yield return new object[]
            {
                new TestCase(
                    testName: "depth < thickness / 2 is unchanged",
                    borderStyle: new(Color.Red, all: 10, depth: 3),
                    expectedResult: new(Color.Red, all: 10, depth: 3)),
            };

            // depth exactly half the thickness - unchanged (still leaves a zero-width, not
            // negative-width, flat band in the middle)
            yield return new object[]
            {
                new TestCase(
                    testName: "depth == thickness / 2 is unchanged",
                    borderStyle: new(Color.Red, all: 10, depth: 5),
                    expectedResult: new(Color.Red, all: 10, depth: 5)),
            };

            // the example from the bug report: thickness 10, depth 8 renders as thickness 10,
            // depth 5 - the *stored* value (asserted separately in RunTestCases) stays 8
            yield return new object[]
            {
                new TestCase(
                    testName: "depth > thickness / 2 clamps to thickness / 2",
                    borderStyle: new(Color.Red, all: 10, depth: 8),
                    expectedResult: new(Color.Red, all: 10, depth: 5)),
            };

            // the actual reported repro values - thickness 25, depth 23
            yield return new object[]
            {
                new TestCase(
                    testName: "reported repro - thickness 25, depth 23 clamps to 12.5",
                    borderStyle: new(Color.Red, all: 25, depth: 23),
                    expectedResult: new(Color.Red, all: 25, depth: 12.5m)),
            };

            // asymmetric thickness - clamp is driven by the *thinnest* side, so corners on that
            // side can't overlap either
            yield return new object[]
            {
                new TestCase(
                    testName: "asymmetric thickness clamps to the thinnest side / 2",
                    borderStyle: new(Color.Red, left: 10, top: 20, right: 10, bottom: 20, depth: 8),
                    expectedResult: new(Color.Red, left: 10, top: 20, right: 10, bottom: 20, depth: 5)),
            };

            // zero thickness - any positive depth clamps down to zero
            yield return new object[]
            {
                new TestCase(
                    testName: "zero thickness clamps depth to zero",
                    borderStyle: new(Color.Red, all: 0, depth: 4),
                    expectedResult: new(Color.Red, all: 0, depth: 0)),
            };

            // depth already zero - unchanged
            yield return new object[]
            {
                new TestCase(
                    testName: "zero depth is unchanged",
                    borderStyle: new(Color.Red, all: 10, depth: 0),
                    expectedResult: new(Color.Red, all: 10, depth: 0)),
            };
        }

        [TestMethod]
        [DynamicData(nameof(GetTestCases))]
        public void RunTestCases(TestCase data)
        {
            var actual = BezelRenderer.ClampDepth(data.BorderStyle);
            var expected = data.ExpectedResult;

            Assert.AreEqual(expected.Color, actual.Color);
            Assert.AreEqual(expected.Left, actual.Left);
            Assert.AreEqual(expected.Top, actual.Top);
            Assert.AreEqual(expected.Right, actual.Right);
            Assert.AreEqual(expected.Bottom, actual.Bottom);
            Assert.AreEqual(expected.Depth, actual.Depth);

            // ClampDepth must never mutate the caller's own BorderStyle (it can't - BorderStyle
            // has no setters - but it must also not just hand the same instance back once it's
            // decided to clamp) - config/settings should keep whatever Depth the user actually
            // entered, even when rendering clamps it.
            if (data.BorderStyle.Depth != expected.Depth)
            {
                Assert.AreNotSame(data.BorderStyle, actual);
            }
        }
    }
}
