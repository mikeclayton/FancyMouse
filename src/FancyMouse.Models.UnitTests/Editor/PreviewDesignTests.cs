using FancyMouse.Models.Editor;

namespace FancyMouse.Models.UnitTests.Editor;

public static class PreviewDesignTests
{
    private static readonly int[] OriginalOrder = [1, 2];

    private static PreviewDesign CreateDesign()
        => new(EditorTestData.Screens, EditorTestData.PreviewStyle);

    private static void AssertPlacement(EditableScreen screen, double x, double y, double scale)
    {
        Assert.AreEqual(x, screen.X, EditorTestData.Delta, "X");
        Assert.AreEqual(y, screen.Y, EditorTestData.Delta, "Y");
        Assert.AreEqual(scale, screen.Scale, EditorTestData.Delta, "Scale");
    }

    [TestClass]
    public sealed class Constructor
    {
        [TestMethod]
        public void PlacesScreensWithTheAutomaticLayout()
        {
            var design = PreviewDesignTests.CreateDesign();

            var content = design.ContentBounds;
            Assert.AreEqual((20m, 20m, 960m, 760m), (content.X, content.Y, content.Width, content.Height));
            PreviewDesignTests.AssertPlacement(design.Screens[0], 20, 280, 0.48);
            PreviewDesignTests.AssertPlacement(design.Screens[1], 500, 280, 0.48);
            Assert.IsFalse(design.Screens.Any(screen => screen.IsCustomised));
        }

        [TestMethod]
        public void NumbersAndStacksScreensInTheOrderGiven()
        {
            var design = PreviewDesignTests.CreateDesign();

            CollectionAssert.AreEqual(PreviewDesignTests.OriginalOrder, design.Screens.Select(screen => screen.Number).ToArray());
            Assert.IsNull(design.SelectedScreen);
        }
    }

    [TestClass]
    public sealed class FrameBounds
    {
        [TestMethod]
        public void WrapsTheScreensInTheCanvasStylesBorderAndPadding()
        {
            var design = PreviewDesignTests.CreateDesign();

            // the screens span (20, 280) - (980, 520), plus 20px of border and padding each side
            var outer = design.FrameBounds.OuterBounds;
            Assert.AreEqual((0m, 260m, 1000m, 280m), (outer.X, outer.Y, outer.Width, outer.Height));
        }

        [TestMethod]
        public void FollowsTheScreensAsTheyMove()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[1];

            design.MoveScreen(screen, 500, 20);

            var content = design.FrameBounds.ContentBounds;
            Assert.AreEqual((20m, 20m, 960m, 500m), (content.X, content.Y, content.Width, content.Height));
        }

        [TestMethod]
        public void ShrinksWhenTheScreensAreMadeSmaller()
        {
            var design = PreviewDesignTests.CreateDesign();
            foreach (var screen in design.Screens)
            {
                screen.Scale = 0.1;
            }

            design.MoveScreen(design.Screens[0], 100, 100);
            design.MoveScreen(design.Screens[1], 200, 100);

            // two 100 x 50 screens side by side - the window would be 200 x 50 plus 40px of chrome
            var size = design.FrameBounds.OuterBounds.Size;
            Assert.AreEqual((240m, 90m), (size.Width, size.Height));
        }

        [TestMethod]
        public void NeverExceedsTheConfiguredPreviewSize()
        {
            var design = PreviewDesignTests.CreateDesign();
            design.MoveScreen(design.Screens[0], -5000, -5000);
            design.MoveScreen(design.Screens[1], 5000, 5000);

            var outer = design.FrameBounds.OuterBounds;
            Assert.AreEqual((0m, 0m, 1000m, 800m), (outer.X, outer.Y, outer.Width, outer.Height));
        }
    }

    [TestClass]
    public sealed class MoveScreen
    {
        [TestMethod]
        public void InsideTheContentArea_MovesExactly()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[0];

            design.MoveScreen(screen, 100, 50);

            PreviewDesignTests.AssertPlacement(screen, 100, 50, 0.48);
            Assert.IsTrue(screen.IsCustomised);
        }

        [TestMethod]
        public void PastTheTopLeft_StopsAtTheContentEdge()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[0];

            design.MoveScreen(screen, -500, -500);

            PreviewDesignTests.AssertPlacement(screen, 20, 20, 0.48);
        }

        [TestMethod]
        public void PastTheBottomRight_StopsAtTheContentEdge()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[0];

            design.MoveScreen(screen, 5000, 5000);

            // content right/bottom (980, 780) less the screen's 480 x 240 size
            PreviewDesignTests.AssertPlacement(screen, 500, 540, 0.48);
        }

        [TestMethod]
        public void LargerThanTheContentArea_PinsToTheTopLeftRatherThanThrowing()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[0];
            screen.Scale = 2; // 2000 x 1000 - e.g. a placement saved under a larger preview size

            design.MoveScreen(screen, 500, 500);

            PreviewDesignTests.AssertPlacement(screen, 20, 20, 2);
        }
    }

    [TestClass]
    public sealed class ResizeScreen
    {
        [TestMethod]
        public void FollowsThePointerAlongWhicheverAxisItMovedFurthest()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[0];
            var (anchorX, anchorY) = PreviewDesign.GetResizeAnchor(screen, ResizeCorner.BottomRight);

            // 600px across is a bigger scale (0.6) than 100px down (0.2), so across wins
            design.ResizeScreen(screen, ResizeCorner.BottomRight, anchorX, anchorY, anchorX + 600, anchorY + 100);

            PreviewDesignTests.AssertPlacement(screen, 20, 280, 0.6);
        }

        [TestMethod]
        public void PastTheContentEdge_StopsAtTheEdge()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[0];
            var (anchorX, anchorY) = PreviewDesign.GetResizeAnchor(screen, ResizeCorner.BottomRight);

            design.ResizeScreen(screen, ResizeCorner.BottomRight, anchorX, anchorY, 5000, 5000);

            // 960px of room to the right (scale 0.96) runs out before 500px below (scale 1.0)
            PreviewDesignTests.AssertPlacement(screen, 20, 280, 0.96);
            Assert.AreEqual(980, screen.X + screen.Width, EditorTestData.Delta);
        }

        [TestMethod]
        public void ByTheTopLeft_KeepsTheBottomRightAnchored()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[1];
            var (anchorX, anchorY) = PreviewDesign.GetResizeAnchor(screen, ResizeCorner.TopLeft);
            Assert.AreEqual((980.0, 520.0), (anchorX, anchorY));

            design.ResizeScreen(screen, ResizeCorner.TopLeft, anchorX, anchorY, 900, 500);

            PreviewDesignTests.AssertPlacement(screen, 900, 480, 0.08);
            Assert.AreEqual(980, screen.X + screen.Width, EditorTestData.Delta);
            Assert.AreEqual(520, screen.Y + screen.Height, EditorTestData.Delta);
        }

        [TestMethod]
        public void ToNothing_StopsWhenTheContentAreaReachesItsMinimum()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[0];
            var (anchorX, anchorY) = PreviewDesign.GetResizeAnchor(screen, ResizeCorner.BottomRight);

            design.ResizeScreen(screen, ResizeCorner.BottomRight, anchorX, anchorY, anchorX, anchorY);

            // the shorter (vertical) axis needs 14px of chrome plus 8px of content - 22 / 500
            Assert.AreEqual(22, screen.Height, EditorTestData.Delta);
            Assert.AreEqual(44, screen.Width, EditorTestData.Delta);
        }
    }

    [TestClass]
    public sealed class ZOrder
    {
        [TestMethod]
        public void SendToBack_MovesTheSelectedScreenToTheBack()
        {
            var design = PreviewDesignTests.CreateDesign();
            var front = design.Screens[1];
            design.Select(front);

            design.SendToBack();

            Assert.AreSame(front, design.Screens[0]);
        }

        [TestMethod]
        public void BringForward_AtTheFront_DoesNothing()
        {
            var design = PreviewDesignTests.CreateDesign();
            var front = design.Screens[1];
            design.Select(front);

            design.BringForward();

            Assert.AreSame(front, design.Screens[1]);
        }

        [TestMethod]
        public void WithNothingSelected_DoesNothing()
        {
            var design = PreviewDesignTests.CreateDesign();
            var before = design.Screens.ToList();

            design.BringToFront();
            design.SendToBack();

            CollectionAssert.AreEqual(before, design.Screens.ToList());
        }
    }

    [TestClass]
    public sealed class Reset
    {
        [TestMethod]
        public void ResetSelectedScreen_RestoresItsPlacementButNotItsStacking()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[1];
            design.Select(screen);
            design.SendToBack();
            design.MoveScreen(screen, 100, 100);

            design.ResetSelectedScreen();

            PreviewDesignTests.AssertPlacement(screen, 500, 280, 0.48);
            Assert.AreSame(screen, design.Screens[0]);
        }

        [TestMethod]
        public void ResetAllScreens_RestoresPlacementsAndStacking()
        {
            var design = PreviewDesignTests.CreateDesign();
            var screen = design.Screens[1];
            design.Select(screen);
            design.SendToBack();
            design.MoveScreen(screen, 100, 100);

            design.ResetAllScreens();

            CollectionAssert.AreEqual(PreviewDesignTests.OriginalOrder, design.Screens.Select(s => s.Number).ToArray());
            PreviewDesignTests.AssertPlacement(screen, 500, 280, 0.48);
        }
    }

    [TestClass]
    public sealed class Selection
    {
        [TestMethod]
        public void ReturnsWhetherTheSelectionChanged()
        {
            var design = PreviewDesignTests.CreateDesign();

            Assert.IsTrue(design.Select(design.Screens[0]));
            Assert.IsFalse(design.Select(design.Screens[0]));
            Assert.IsTrue(design.Select(null));
        }

        [TestMethod]
        public void AScreenFromAnotherDesign_Throws()
        {
            var design = PreviewDesignTests.CreateDesign();
            var other = PreviewDesignTests.CreateDesign();

            Assert.ThrowsExactly<ArgumentException>(() => design.Select(other.Screens[0]));
        }
    }
}
