using System.Diagnostics;
using FancyMouse.Helpers;
using FancyMouse.PerfTests.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FancyMouse.PerfTests;

[TestClass]
public class PerfTests
{
    public static void RunPerfTest(Func<Rectangle, IEnumerable<Rectangle>, Size, Bitmap> testMethod)
    {
        var times = new List<long>();
        var screens = Screen.AllScreens;
        var screenBounds = screens.Select(screen => screen.Bounds).ToList();
        var desktopBounds = screenBounds.GetBoundingRectangle();
        var screenshotSize = new Size(desktopBounds.Width / 40, desktopBounds.Height / 40);
        var count = 100;
        for (var i = 0; i < count; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var preview = testMethod(desktopBounds, screenBounds, screenshotSize);
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
    [TestCategory("Performance")]
    public void CsWin32JigsawScreenCopyHelper_PerfTest()
    {
        // PerfTests.RunPerfTest(
        //    CsWin32JigsawScreenCopyHelper.CopyFromScreen);
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void GraphicsScreenCopyHelper_PerfTest()
    {
        PerfTests.RunPerfTest(
            (desktopBounds, screenBounds, screenshotSize) =>
                GraphicsScreenCopyHelper.CopyFromScreen(desktopBounds));
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void ParallelStretchBltScreenCopyHelper_PerfTest()
    {
        // PerfTests.RunPerfTest(
        //    ParallelStretchBltScreenCopyHelper.CopyFromScreen);
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void ScalingScreenCopyHelper_PerfTest()
    {
        PerfTests.RunPerfTest(
            ScalingScreenCopyHelper.CopyFromScreen);
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void StretchBltScreenCopyHelper_PerfTest()
    {
        // PerfTests.RunPerfTest(
        //     StretchBltScreenCopyHelper.CopyFromScreen);
    }

    private static IEnumerable<IGrouping<long, long>> Bucket(List<long> values, int bucketSize)
    {
        return values.GroupBy(value => value / bucketSize);
    }

    private static long Median(List<long> values)
    {
        return values[values.Count / 2];
    }

    private static long Mode(List<long> values, int bucketSize)
    {
        return Bucket(values, bucketSize).OrderByDescending(group => group.Count()).First().Key * bucketSize;
    }
}
