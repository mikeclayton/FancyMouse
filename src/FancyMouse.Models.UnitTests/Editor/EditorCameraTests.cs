using FancyMouse.Models.Drawing;
using FancyMouse.Models.Editor;

namespace FancyMouse.Models.UnitTests.Editor;

public static class EditorCameraTests
{
    /// <summary>
    /// A camera over a 3000 x 3000 canvas centred on a 1000 x 800 preview, with no fit padding -
    /// so in a 500 x 400 viewport the preview fits exactly at half size.
    /// </summary>
    private static EditorCamera CreateCamera(double viewportWidth = 500, double viewportHeight = 400)
    {
        var camera = new EditorCamera(
            canvasBounds: new RectangleInfo(-1000, -1100, 3000, 3000),
            fitBounds: new RectangleInfo(0, 0, 1000, 800),
            fitPadding: 0);
        camera.SetViewportSize(viewportWidth, viewportHeight);
        return camera;
    }

    [TestClass]
    public sealed class SetViewportSize
    {
        [TestMethod]
        public void BeforeTheUserMovesTheCamera_ZoomsToFitAndCentres()
        {
            var camera = EditorCameraTests.CreateCamera(viewportWidth: 600, viewportHeight: 400);

            Assert.AreEqual(0.5, camera.Zoom, EditorTestData.Delta);
            Assert.AreEqual(50, camera.OffsetX, EditorTestData.Delta);
            Assert.AreEqual(0, camera.OffsetY, EditorTestData.Delta);
            Assert.IsFalse(camera.IsMoved);
        }

        [TestMethod]
        public void AfterTheUserMovesTheCamera_DoesNotRefit()
        {
            var camera = EditorCameraTests.CreateCamera();
            camera.ZoomAt(250, 200, 2);

            camera.SetViewportSize(1000, 800);

            Assert.AreEqual(1, camera.Zoom, EditorTestData.Delta);
            Assert.IsTrue(camera.IsMoved);
        }

        [TestMethod]
        public void WithNoViewportYet_DoesNotThrow()
        {
            var camera = EditorCameraTests.CreateCamera(viewportWidth: 0, viewportHeight: 0);

            camera.ZoomAt(0, 0, 2);
            camera.PanBy(10, 10);
        }
    }

    [TestClass]
    public sealed class ZoomAt
    {
        [TestMethod]
        public void KeepsThePreviewPointUnderThePointerWhereItIs()
        {
            var camera = EditorCameraTests.CreateCamera();
            var before = camera.ToPreview(120, 80);

            camera.ZoomAt(120, 80, 1.5);

            var after = camera.ToPreview(120, 80);
            Assert.AreEqual(0.75, camera.Zoom, EditorTestData.Delta);
            Assert.AreEqual(before.X, after.X, EditorTestData.Delta);
            Assert.AreEqual(before.Y, after.Y, EditorTestData.Delta);
        }

        [TestMethod]
        public void ZoomingInTooFar_StopsAtTheMaximum()
        {
            var camera = EditorCameraTests.CreateCamera();

            camera.ZoomAt(250, 200, 1000);

            Assert.AreEqual(EditorCamera.MaximumZoom, camera.Zoom, EditorTestData.Delta);
        }

        [TestMethod]
        public void ZoomingOutTooFar_StopsWithTheWholeCanvasInView()
        {
            var camera = EditorCameraTests.CreateCamera();

            camera.ZoomAt(250, 200, 0.0001);

            // the viewport's shorter side (400) over the canvas's 3000
            Assert.AreEqual(400.0 / 3000, camera.Zoom, EditorTestData.Delta);
        }
    }

    [TestClass]
    public sealed class ZoomTo
    {
        [TestMethod]
        public void ZoomsAboutTheMiddleOfTheViewport()
        {
            var camera = EditorCameraTests.CreateCamera();
            var before = camera.ToPreview(250, 200);

            camera.ZoomTo(2);

            var after = camera.ToPreview(250, 200);
            Assert.AreEqual(2, camera.Zoom, EditorTestData.Delta);
            Assert.AreEqual(before.X, after.X, EditorTestData.Delta);
            Assert.AreEqual(before.Y, after.Y, EditorTestData.Delta);
        }

        [TestMethod]
        public void PastTheMaximum_StopsAtTheMaximum()
        {
            var camera = EditorCameraTests.CreateCamera();

            camera.ZoomTo(1000);

            Assert.AreEqual(EditorCamera.MaximumZoom, camera.Zoom, EditorTestData.Delta);
        }
    }

    [TestClass]
    public sealed class PanBy
    {
        [TestMethod]
        public void MovesTheCameraByTheGivenAmount()
        {
            var camera = EditorCameraTests.CreateCamera();

            camera.PanBy(30, -20);

            Assert.AreEqual(30, camera.OffsetX, EditorTestData.Delta);
            Assert.AreEqual(-20, camera.OffsetY, EditorTestData.Delta);
            Assert.IsTrue(camera.IsMoved);
        }

        [TestMethod]
        public void PanningOffTheCanvas_KeepsTheMiddleOfTheViewportOverIt()
        {
            var camera = EditorCameraTests.CreateCamera();

            camera.PanBy(1_000_000, 1_000_000);

            var middle = camera.ToPreview(250, 200);
            Assert.AreEqual(-1000, middle.X, EditorTestData.Delta);
            Assert.AreEqual(-1100, middle.Y, EditorTestData.Delta);
        }
    }

    [TestClass]
    public sealed class ZoomToFit
    {
        [TestMethod]
        public void RestoresTheFittedViewAndForgetsTheUserMovedIt()
        {
            var camera = EditorCameraTests.CreateCamera();
            camera.ZoomAt(100, 100, 3);
            camera.PanBy(40, 40);

            camera.ZoomToFit();

            Assert.AreEqual(0.5, camera.Zoom, EditorTestData.Delta);
            Assert.AreEqual(0, camera.OffsetX, EditorTestData.Delta);
            Assert.AreEqual(0, camera.OffsetY, EditorTestData.Delta);
            Assert.IsFalse(camera.IsMoved);
        }
    }

    [TestClass]
    public sealed class ToView
    {
        [TestMethod]
        public void IsTheInverseOfToPreview()
        {
            var camera = EditorCameraTests.CreateCamera();
            camera.ZoomAt(130, 70, 1.7);

            var view = camera.ToView(123, 456, 0, 0);
            var preview = camera.ToPreview(view.X, view.Y);

            Assert.AreEqual(123, preview.X, EditorTestData.Delta);
            Assert.AreEqual(456, preview.Y, EditorTestData.Delta);
        }
    }
}
