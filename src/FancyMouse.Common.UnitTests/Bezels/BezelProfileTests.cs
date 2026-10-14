using FancyMouse.Common.Bezels;

namespace FancyMouse.Common.UnitTests.Bezels;

public static class BezelProfileTests
{
    [TestClass]
    public sealed class BezelProfileCurvedTests
    {
        public sealed class NormalTestCase
        {
            public NormalTestCase(string testName, int n, int d, double position, double expectedNormal)
            {
                this.TestName = testName;
                this.N = n;
                this.D = d;
                this.Position = position;
                this.ExpectedNormal = expectedNormal;
            }

            public string TestName { get; }

            public int N { get; }

            public int D { get; }

            public double Position { get; }

            public double ExpectedNormal { get; }

            public override string ToString() => this.TestName;
        }

        public static IEnumerable<object[]> GetNormalTestCases()
        {
            // n=20, d=5 - outer ring [0,5), flat zone [5,15), inner ring [15,20)
            yield return new object[] { new NormalTestCase("outer arc edge - full highlight", 20, 5, 0.0, 0.0) };
            yield return new object[] { new NormalTestCase("outer ring midpoint - arccos(0.5) sweep", 20, 5, 2.5, Math.PI / 3.0) };
            yield return new object[] { new NormalTestCase("outer ring / flat zone boundary", 20, 5, 5.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("flat zone midpoint - no effect", 20, 5, 10.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("flat zone / inner ring boundary", 20, 5, 15.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("inner ring midpoint - arccos(0.5) sweep", 20, 5, 17.5, 2.0 * Math.PI / 3.0) };
            yield return new object[] { new NormalTestCase("content boundary - full shadow", 20, 5, 20.0, Math.PI) };

            // n=10, d=5 - depth is half the width (BezelRenderer.ClampDepth's own max), so the
            // flat zone has zero width and outer/inner rings meet at a single continuous point
            yield return new object[] { new NormalTestCase("zero-width flat zone - outer/inner ring boundary", 10, 5, 5.0, Math.PI / 2.0) };

            // d=0 - no effect ring at all (e.g. a bezel with 3D depth disabled). There's no
            // bevel surface to have a normal other than flat, even right at position 0 - a
            // regression once caused a highlight rim to appear on zero-depth bezels because
            // the "position <= 0 = facing the light" branch fired unconditionally. See
            // BezelProfileCurved.GetProfileNormal's _d <= 0 guard.
            yield return new object[] { new NormalTestCase("zero-depth ring - outer arc edge is flat, not highlighted", 20, 0, 0.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("zero-depth ring - interior is flat", 20, 0, 10.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("zero-depth ring - content boundary is flat, not shadowed", 20, 0, 20.0, Math.PI / 2.0) };
        }

        [TestMethod]
        [DynamicData(nameof(GetNormalTestCases))]
        public void GetProfileNormal_ReturnsExpectedAngle(NormalTestCase data)
        {
            var profile = new BezelProfileCurved(data.N, data.D);
            var proportion = data.Position / data.N;
            var actual = profile.GetProfileNormal(proportion);
            Assert.AreEqual(data.ExpectedNormal, actual, 1e-9);
        }

        [TestMethod]
        [DataRow(-0.1)]
        [DataRow(1.1)]
        public void GetProfileNormal_ThrowsForProportionOutsideZeroToOne(double proportion)
        {
            var profile = new BezelProfileCurved(20, 5);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => profile.GetProfileNormal(proportion));
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(1.0)]
        public void GetProfileNormal_AcceptsInclusiveBounds(double proportion)
        {
            var profile = new BezelProfileCurved(20, 5);
            _ = profile.GetProfileNormal(proportion);
        }
    }

    [TestClass]
    public sealed class BezelProfileRampedTests
    {
        public sealed class NormalTestCase
        {
            public NormalTestCase(string testName, int n, int d, double rampAngleDegrees, double position, double expectedNormal)
            {
                this.TestName = testName;
                this.N = n;
                this.D = d;
                this.RampAngleDegrees = rampAngleDegrees;
                this.Position = position;
                this.ExpectedNormal = expectedNormal;
            }

            public string TestName { get; }

            public int N { get; }

            public int D { get; }

            public double RampAngleDegrees { get; }

            public double Position { get; }

            public double ExpectedNormal { get; }

            public override string ToString() => this.TestName;
        }

        public static IEnumerable<object[]> GetNormalTestCases()
        {
            // n=20, d=5, rampAngle=30 deg (pi/6) - outer ramp [0,5), flat section [5,15], inner ramp (15,20]
            var rampAngle = Math.PI / 6.0;
            yield return new object[] { new NormalTestCase("outer ramp - constant ramp angle throughout", 20, 5, 30.0, 0.0, (Math.PI / 2.0) - rampAngle) };
            yield return new object[] { new NormalTestCase("outer ramp midpoint - same constant angle", 20, 5, 30.0, 2.5, (Math.PI / 2.0) - rampAngle) };
            yield return new object[] { new NormalTestCase("flat section start - no effect", 20, 5, 30.0, 5.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("flat section midpoint - no effect", 20, 5, 30.0, 10.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("flat section end - no effect (mirrors flat section start)", 20, 5, 30.0, 15.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("inner ramp midpoint - constant ramp angle throughout", 20, 5, 30.0, 17.5, (Math.PI / 2.0) + rampAngle) };
            yield return new object[] { new NormalTestCase("inner bezel edge - same constant angle", 20, 5, 30.0, 20.0, (Math.PI / 2.0) + rampAngle) };

            // n=10, d=5 - depth is half the width, so the flat section shrinks to a single
            // point between the two ramps (unlike BezelProfileCurved, there's no continuous
            // handoff at this boundary - the profile steps from one ramp angle to the other)
            yield return new object[] { new NormalTestCase("zero-width flat section - boundary point is flat", 10, 5, 30.0, 5.0, Math.PI / 2.0) };

            // d=0 - no ramps at all, so the whole profile is flat, including both edges
            // (pins the flat section's "<=" - with "<", the inner bezel edge would get
            // the inner ramp's shadow angle even though there's no ramp there)
            yield return new object[] { new NormalTestCase("zero-depth ramps - outer bezel edge is flat, not highlighted", 20, 0, 30.0, 0.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("zero-depth ramps - interior is flat", 20, 0, 30.0, 10.0, Math.PI / 2.0) };
            yield return new object[] { new NormalTestCase("zero-depth ramps - inner bezel edge is flat, not shadowed", 20, 0, 30.0, 20.0, Math.PI / 2.0) };
        }

        [TestMethod]
        [DynamicData(nameof(GetNormalTestCases))]
        public void GetProfileNormal_ReturnsExpectedAngle(NormalTestCase data)
        {
            var profile = new BezelProfileRamped(data.N, data.D, data.RampAngleDegrees);
            var proportion = data.Position / data.N;
            var actual = profile.GetProfileNormal(proportion);
            Assert.AreEqual(data.ExpectedNormal, actual, 1e-9);
        }

        [TestMethod]
        [DataRow(-0.1)]
        [DataRow(1.1)]
        public void GetProfileNormal_ThrowsForProportionOutsideZeroToOne(double proportion)
        {
            var profile = new BezelProfileRamped(20, 5, 30.0);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => profile.GetProfileNormal(proportion));
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(1.0)]
        public void GetProfileNormal_AcceptsInclusiveBounds(double proportion)
        {
            var profile = new BezelProfileRamped(20, 5, 30.0);
            _ = profile.GetProfileNormal(proportion);
        }
    }
}
