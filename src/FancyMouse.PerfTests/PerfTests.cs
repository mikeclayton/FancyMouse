using FancyMouse.PerfTests.Helpers;
using FancyMouse.PerfTests.ScreenCopying;
using FancyMouse.ScreenCopying;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace FancyMouse.PerfTests
{

    [TestClass]
    public class PerfTests
    {

        private static void RunPerfTest(ICopyFromScreen copyHelper)
        {
            var times = new List<long>();

            var screens = Screen.AllScreens;
            var screenBounds = screens.Select(screen => screen.Bounds).ToList();
            var desktopBounds = LayoutHelper.CombineRegions(screenBounds);
            var screenshotSize = new Size(desktopBounds.Width / 4, desktopBounds.Height / 4);

            var count = 100;
            for (var i = 0; i < count; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                var preview = copyHelper.CopyFromScreen(desktopBounds, screenBounds, screenshotSize);
                stopwatch.Stop();
                times.Add(stopwatch.ElapsedMilliseconds);
                preview.Dispose();
            }

            Console.WriteLine("count = {0}", count);
            Console.WriteLine("mean = {0}", times.Average());
            Console.WriteLine("median = {0}", PerfTests.Median(times));
            Console.WriteLine("mode = {0}", PerfTests.Mode(times, 50));
            Console.WriteLine("times = {0}", string.Join("\r\n", times));

        }

        [TestMethod]
        public void ScalingScreenCopyHelper_PerfTest()
        {
            PerfTests.RunPerfTest(new ScalingScreenCopyHelper());
        }

        [TestMethod]
        public void StretchBltScreenCopyHelper_PerfTest()
        {
            PerfTests.RunPerfTest(new StretchBltScreenCopyHelper());
        }

        [TestMethod]
        public void CsWin32JigsawScreenCopyHelper_PerfTest()
        {
            PerfTests.RunPerfTest(new CsWin32JigsawScreenCopyHelper());
        }

        [TestMethod]
        public void NativeJigsawScreenCopyHelper_PerfTest()
        {
            PerfTests.RunPerfTest(new NativeJigsawScreenCopyHelper());
        }

        [TestMethod]
        public void ParallelJigsawScreenCopyHelper_PerfTest()
        {
            PerfTests.RunPerfTest(new ParallelJigsawScreenCopyHelper());
        }

        private static IEnumerable<IGrouping<long, long>> Bucket(List<long> values, int bucketSize)
        {
            return values.GroupBy(value => value / bucketSize);
        }

        private static long Median(List<long> values)
        {
            return values[values.Count / 2];
        }

        private static  long Mode(List<long> values, int bucketSize)
        {
            return Bucket(values, bucketSize).OrderByDescending(group => group.Count()).First().Key * bucketSize;
        }

    }

}