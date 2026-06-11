using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using FancyMouse.Drawing.Bezels;

public static class Program
{
    public static void Main()
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // Colour palette
        // ─────────────────────────────────────────────────────────────────────────────
#pragma warning disable SA1312
        var BG_COLOR = Color.FromArgb(218, 218, 220);  // light grey canvas background
        var OUTER_BEZEL = Color.FromArgb(48, 68, 112); // dark blue outer frame border
        var INNER_BG1 = Color.FromArgb(58, 88, 150);   // frame content gradient — top-left
        var INNER_BG2 = Color.FromArgb(38, 62, 118);   // frame content gradient — bottom-right
        var SCREEN_BEZEL = Color.Green; // Color.FromArgb(52, 54, 58); // dark grey monitor bezel
        var SCREEN_INNER = Color.FromArgb(42, 44, 56); // dark screen content area
#pragma warning restore SA1312

        // ─────────────────────────────────────────────────────────────────────────────
        // 3-D effect constants
        // (declared as variables so they can be passed to DrawBezel / tweaked per bezel)
        // ─────────────────────────────────────────────────────────────────────────────
        const double FADE_START = 30.0;         // degrees from edge where corner rolloff begins
        const double FADE_END = 60.0;           // degrees where rolloff reaches zero
        const double HL_MAX = 0x44 / 255.0;     // peak highlight opacity fraction (~26.7 %)
        const double SH_MAX = 0x44 / 255.0;     // peak shadow   opacity fraction (~26.7 %)
        const float EDGE_FADE_FRACTION = 0.75f; // how far along a vertical edge the secondary effect runs

        // ═════════════════════════════════════════════════════════════════════════════
        // Render
        // ═════════════════════════════════════════════════════════════════════════════

        // Image dimensions
        const int W_IMG = 1050;
        const int H_IMG = 700;

        // Outer frame geometry
        const int BORDER = 44; // border ring width
        const int DEPTH = 6;   // number of 3-D effect depth layers

        // Screen bezel geometry
        const int SCREEN_BORDER = 16; // screen border ring width
        const int SCREEN_DEPTH = 3;   // screen depth layers
        const int PAD = 24;           // gap between outer frame and screens, and between screens

        using var bmp = new Bitmap(W_IMG, H_IMG, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;

        // Geometry
        int mbx = 60, mby = 60;
        int mbw = W_IMG - (2 * mbx);
        int mbh = H_IMG - (2 * mby);

        int sbx = mbx + BORDER + PAD;
        int sby = mby + BORDER + PAD;
        int sbw = 400; // (mbw - BORDER * 2 - PAD * 3) / 2;
        int sbh = 300; // mbh - BORDER * 2 - PAD * 2;

        // Draw
        g.Clear(BG_COLOR);

        BezelGraphics.DrawFrameBackground(g, mbx, mby, mbw, mbh, DEPTH, INNER_BG1, INNER_BG2);

        // NOTE: do NOT draw a flat bezel ring over the outer frame here — the grey ring's dark
        // colour would show through the partial-alpha pixels at the outer arc edge of the atlas
        // corners, making them look grey-fringed instead of blending cleanly into the canvas.
        var renderer = new BezelRenderer(
            bezelColor: SCREEN_BEZEL,
            bezelThickness: SCREEN_BORDER,
            threeDEffectDepth: SCREEN_DEPTH,
            fadeStart: FADE_START,
            fadeEnd: FADE_END,
            hlMax: HL_MAX,
            shMax: SH_MAX,
            edgeFadeFraction: EDGE_FADE_FRACTION);

        BezelGraphics.DrawScreenBackground(g, sbx, sby, sbw, sbh, SCREEN_BORDER, SCREEN_INNER);
        renderer.DrawBezel(g, sbx, sby, sbw, sbh);

        // BezelGraphics.DrawScreenBackground(g, sbx + sbw + PAD, sby, sbw, sbh, SCREEN_BORDER, SCREEN_RADIUS, SCREEN_INNER);
        // renderer.DrawBezel(g, sbx + sbw + PAD, sby, sbw, sbh);
        bmp.Save(@"C:\temp\mousejump_preview.png", ImageFormat.Png);

        // save a sample screen bezel
        var screen = new Bitmap(400, 300, PixelFormat.Format32bppArgb);
        using var screenG = Graphics.FromImage(screen);
        BezelGraphics.DrawFlatBezelRing(screenG, 0, 0, 400, 300, SCREEN_BORDER, 0, SCREEN_BEZEL);
        screen.Save(@"C:\temp\mousejump_screen_bezel_solid.png", ImageFormat.Png);

        // ── Performance timing ────────────────────────────────────────────────────────
        // The render above has already JIT-compiled everything; these runs measure
        // steady-state cost only.
        const int TIMING_RUNS = 100;
        var sw = new Stopwatch();

        sw.Restart();
        for (int i = 0; i < TIMING_RUNS; i++)
        {
            BezelGraphics.DrawScreenBackground(g, sbx, sby, sbw, sbh, SCREEN_BORDER, SCREEN_INNER);
            renderer.DrawBezel(g, sbx, sby, sbw, sbh);
        }

        sw.Stop();
        double screenMs = sw.Elapsed.TotalMilliseconds / TIMING_RUNS;

        Console.WriteLine($"DrawScreenBackground + DrawBezel : {screenMs,7:F3} ms  (avg of {TIMING_RUNS} runs)");
    }
}
