using FancyMouse.Models.Editor;

namespace FancyMouse.Models.UnitTests.Editor;

/// <summary>
/// Gesture tests - these drive <see cref="PreviewDesigner"/> the way a view would, with pointer
/// positions in view units.
/// </summary>
public static class PreviewDesignerTests
{
    private static PreviewDesigner CreateDesigner()
    {
        var designer = new PreviewDesigner(EditorTestData.Screens, EditorTestData.PreviewStyle);
        designer.Camera.SetViewportSize(1048, 848);
        return designer;
    }

    /// <summary>
    /// Gets the middle of <paramref name="screen"/> in view units.
    /// </summary>
    private static (double X, double Y) MiddleOf(PreviewDesigner designer, EditableScreen screen)
    {
        var bounds = designer.Camera.ToView(screen);
        return (bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));
    }

    [TestClass]
    public sealed class MovingAScreen
    {
        [TestMethod]
        public void PressingAScreen_SelectsIt()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            var screen = designer.Design.Screens[0];
            var raised = 0;
            designer.SelectionChanged += (sender, e) => raised++;

            var (x, y) = PreviewDesignerTests.MiddleOf(designer, screen);
            designer.PressScreen(screen, x, y);

            Assert.AreSame(screen, designer.Design.SelectedScreen);
            Assert.AreEqual(1, raised);
            Assert.IsTrue(designer.IsGestureInProgress);
        }

        [TestMethod]
        public void Dragging_MovesTheScreenByThePointersDistanceInPreviewPixels()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            var screen = designer.Design.Screens[0];
            var zoom = designer.Camera.Zoom;

            var (x, y) = PreviewDesignerTests.MiddleOf(designer, screen);
            designer.PressScreen(screen, x, y);
            designer.MovePointer(x + 30, y - 20);
            designer.ReleasePointer();

            Assert.AreEqual(20 + (30 / zoom), screen.X, EditorTestData.Delta);
            Assert.AreEqual(280 - (20 / zoom), screen.Y, EditorTestData.Delta);
            Assert.IsFalse(designer.IsGestureInProgress);
        }

        [TestMethod]
        public void ZoomingMidDrag_KeepsTheScreenUnderThePointer()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            var screen = designer.Design.Screens[0];

            var (x, y) = PreviewDesignerTests.MiddleOf(designer, screen);
            designer.PressScreen(screen, x, y);
            designer.TurnWheel(x, y, 3);
            designer.MovePointer(x, y);

            // the wheel zooms about the pointer, so the same view point is still the same
            // preview point - and the screen hasn't moved
            Assert.AreEqual(20, screen.X, EditorTestData.Delta);
            Assert.AreEqual(280, screen.Y, EditorTestData.Delta);
        }
    }

    [TestClass]
    public sealed class ResizingAScreen
    {
        [TestMethod]
        public void WithNothingSelected_DoesNothing()
        {
            var designer = PreviewDesignerTests.CreateDesigner();

            designer.PressResizeHandle(ResizeCorner.BottomRight, 100, 100);

            Assert.IsFalse(designer.IsGestureInProgress);
        }

        [TestMethod]
        public void DraggingACorner_ResizesTheSelectedScreen()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            var screen = designer.Design.Screens[0];
            designer.Select(screen);
            var corner = designer.Camera.ToView(screen);

            designer.PressResizeHandle(ResizeCorner.BottomRight, corner.Right, corner.Bottom);
            designer.MovePointer(corner.Right + (120 * designer.Camera.Zoom), corner.Bottom);
            designer.ReleasePointer();

            // 120 preview pixels wider than 480, with the top-left corner left where it was
            Assert.AreEqual(600, screen.Width, EditorTestData.Delta);
            Assert.AreEqual(20, screen.X, EditorTestData.Delta);
            Assert.AreEqual(280, screen.Y, EditorTestData.Delta);
        }
    }

    [TestClass]
    public sealed class ZoomCommands
    {
        [TestMethod]
        public void ZoomInThenZoomOut_RestoresTheZoom()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            var zoom = designer.Camera.Zoom;

            designer.ZoomIn();
            Assert.AreEqual(zoom * PreviewDesigner.ZoomStepFactor, designer.Camera.Zoom, EditorTestData.Delta);
            designer.ZoomOut();

            Assert.AreEqual(zoom, designer.Camera.Zoom, EditorTestData.Delta);
        }

        [TestMethod]
        public void ZoomToActualSize_ShowsOnePreviewPixelPerPhysicalPixel()
        {
            var designer = PreviewDesignerTests.CreateDesigner();

            // a view in DIPs on a monitor at 150% scaling
            designer.ZoomToActualSize(1 / 1.5);

            Assert.AreEqual(1 / 1.5, designer.Camera.Zoom, EditorTestData.Delta);
        }
    }

    [TestClass]
    public sealed class PressingTheBackground
    {
        [TestMethod]
        public void AClick_ClearsTheSelection()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            designer.Select(designer.Design.Screens[0]);

            designer.PressBackground(5, 5);
            designer.ReleasePointer();

            Assert.IsNull(designer.Design.SelectedScreen);
        }

        [TestMethod]
        public void AJitterWithinThePanThreshold_IsStillAClick()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            designer.Select(designer.Design.Screens[0]);
            var (offsetX, offsetY) = (designer.Camera.OffsetX, designer.Camera.OffsetY);

            designer.PressBackground(5, 5);
            designer.MovePointer(5 + PreviewDesigner.PanThreshold, 5);
            designer.ReleasePointer();

            Assert.IsNull(designer.Design.SelectedScreen);
            Assert.AreEqual(offsetX, designer.Camera.OffsetX, EditorTestData.Delta);
            Assert.AreEqual(offsetY, designer.Camera.OffsetY, EditorTestData.Delta);
        }

        [TestMethod]
        public void ADrag_PansTheCameraAndKeepsTheSelection()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            var screen = designer.Design.Screens[0];
            designer.Select(screen);
            var offsetX = designer.Camera.OffsetX;

            designer.PressBackground(5, 5);
            designer.MovePointer(25, 5);
            designer.MovePointer(45, 5);
            designer.ReleasePointer();

            // the first move crosses the threshold, and pans from where the previous move left off
            Assert.AreEqual(offsetX + 40, designer.Camera.OffsetX, EditorTestData.Delta);
            Assert.AreSame(screen, designer.Design.SelectedScreen);
        }

        [TestMethod]
        public void Cancelled_IsNotAClick()
        {
            var designer = PreviewDesignerTests.CreateDesigner();
            var screen = designer.Design.Screens[0];
            designer.Select(screen);

            designer.PressBackground(5, 5);
            designer.CancelGesture();

            Assert.AreSame(screen, designer.Design.SelectedScreen);
            Assert.IsFalse(designer.IsGestureInProgress);
        }
    }
}
