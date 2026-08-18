using System.Drawing;

using FancyMouse.Common.Helpers;
using FancyMouse.Models.Display;

namespace FancyMouse.Common.Blurring;

/// <summary>
/// Generates and retains a blurred stand-in screenshot per physical screen, for use as an
/// activation's initial placeholder while its own fresh screenshot is still loading - see
/// <see cref="BlurHelper.CreateBlurredCopy"/> for what "blurred" means here. Keyed by
/// <see cref="ScreenInfo"/> (the physical screen) rather than any per-activation layout
/// position, so a monitor keeps its own cached blur across activations regardless of where it
/// falls in any particular activation's screen list - fixes a bug where a positionally-indexed
/// cache could show one monitor's stale content under a *different* monitor's bezel after the
/// connected monitor set changed between activations.
/// </summary>
/// <remarks>
/// Each screen has three slots: at most one blur actually running ("doing"), at most one
/// screenshot waiting for its turn ("todo" - only ever the latest; an older queued one is
/// discarded rather than piling up), and at most one completed result ("done", served by
/// <see cref="TryGet"/> until it expires). A "doing" blur is never interrupted once started -
/// only ever superseded via "todo" - so blur work always eventually finishes and "done" keeps
/// converging on something recent even under rapid repeat activation, rather than every new
/// activation cancelling the previous one before any ever completes.
/// </remarks>
public sealed class ScreenshotBlurPipeline
{
    /// <summary>
    /// How strongly a stand-in screenshot is blurred, desaturated and darkened - see
    /// <see cref="BlurHelper.CreateBlurredCopy"/> for what the values mean. Muting the stand-in
    /// on top of blurring it is what makes the real screenshot visually "pop" into place once it
    /// arrives, rather than the swap reading as a same-ish image just sharpening.
    /// </summary>
    private const decimal BlurIntensity = 0.75m;
    private const double BlurSaturation = 0.5;
    private const double BlurBrightness = 0.55;

    /// <summary>
    /// How long a completed blur remains available - see <see cref="TryGet"/>.
    /// </summary>
    private static readonly TimeSpan ClaimWindow = TimeSpan.FromMinutes(2);

    private readonly object sync = new();
    private readonly Dictionary<ScreenInfo, ScreenshotBlurState> screens = new();

    /// <summary>
    /// <see cref="global::FancyMouse.Common.Telemetry.Telemetry.Current"/>, renamed here because
    /// "Telemetry" (the class) collides with the sibling <c>FancyMouse.Common.Telemetry</c>
    /// namespace - a plain <c>using</c> can't fix this (enclosing-namespace lookup wins over
    /// using-aliases for a name reachable that way), so this avoids needing every call site
    /// below to fully-qualify it instead.
    /// </summary>
    private static global::FancyMouse.Common.Telemetry.TelemetryContext CurrentTelemetry
        => global::FancyMouse.Common.Telemetry.Telemetry.Current;

    /// <summary>
    /// Supplies a fresh screenshot for <paramref name="screenInfo"/>. Takes ownership of
    /// <paramref name="screenshotImage"/> - see <see cref="Capture.IScreenshotCaptureSink.SetScreenshotAsync"/>
    /// for the same convention this mirrors. If <paramref name="screenInfo"/> isn't one of the
    /// currently active screens (see <see cref="SetActiveScreens"/>), disposes
    /// <paramref name="screenshotImage"/> and returns - a stale, superseded screen has nowhere
    /// left to go. Otherwise, either starts blurring it immediately (nothing already in
    /// progress for this screen) or queues it as this screen's "todo" - replacing, and
    /// disposing, whatever was previously queued, since only the latest matters.
    /// </summary>
    public void SetScreenshot(ScreenInfo screenInfo, Bitmap screenshotImage)
    {
        ArgumentNullException.ThrowIfNull(screenInfo);
        ArgumentNullException.ThrowIfNull(screenshotImage);

        lock (this.sync)
        {
            if (!this.screens.TryGetValue(screenInfo, out var state))
            {
                screenshotImage.Dispose();
                return;
            }

            ScreenshotBlurPipeline.CurrentTelemetry.WriteEvent(new { handle = screenInfo.Handle }, "screenshotBlurQueued");

            if (state.BlurInProgress)
            {
                state.Todo?.Dispose();
                state.Todo = screenshotImage;
                return;
            }

            state.BlurInProgress = true;
            _ = Task.Run(() => this.RunBlur(screenInfo, screenshotImage));
        }
    }

    /// <summary>
    /// If a blurred stand-in for <paramref name="screenInfo"/> is available and still within
    /// <see cref="ClaimWindow"/> of completing, invokes <paramref name="use"/> with it and
    /// returns <see langword="true"/> - otherwise returns <see langword="false"/> without
    /// invoking <paramref name="use"/>. <paramref name="use"/> runs synchronously while this
    /// pipeline's own lock is still held, directly against its retained copy (no clone) - it
    /// must not retain the <see cref="Bitmap"/> passed to it beyond the call, since the pipeline
    /// is free to dispose it the moment <paramref name="use"/> returns (e.g. once superseded by
    /// a newer completed blur). The same completed blur may still be passed to a later call
    /// before it expires or is superseded. If a blur for this screen is still in progress, it's
    /// left running (see the class remarks) - this call only ever reports what's already
    /// finished. Records which of three outcomes happened - "found" (a fresh completed blur was
    /// passed to <paramref name="use"/>), "not complete" (this screen has a blur queued or
    /// running, just not ready yet), or "not found" (nothing tracked for this screen at all) - to
    /// make the caching behaviour directly observable in telemetry.
    /// </summary>
    public bool TryGet(ScreenInfo screenInfo, Action<Bitmap> use)
    {
        ArgumentNullException.ThrowIfNull(screenInfo);
        ArgumentNullException.ThrowIfNull(use);

        lock (this.sync)
        {
            if (!this.screens.TryGetValue(screenInfo, out var state))
            {
                ScreenshotBlurPipeline.CurrentTelemetry.WriteEvent(new { handle = screenInfo.Handle }, "blurRequestedNotFound");
                return false;
            }

            if (state.Done is not null)
            {
                if (DateTime.UtcNow - state.DoneAt <= ScreenshotBlurPipeline.ClaimWindow)
                {
                    ScreenshotBlurPipeline.CurrentTelemetry.WriteEvent(new { handle = screenInfo.Handle }, "blurRequestedFound");
                    use(state.Done);
                    return true;
                }

                state.Done.Dispose();
                state.Done = null;
            }

            var operation = (state.BlurInProgress || state.Todo is not null)
                ? "blurRequestedNotComplete"
                : "blurRequestedNotFound";
            ScreenshotBlurPipeline.CurrentTelemetry.WriteEvent(new { handle = screenInfo.Handle }, operation);
            return false;
        }
    }

    /// <summary>
    /// Reconciles tracked state against <paramref name="currentScreens"/> - the full set of
    /// currently connected physical screens, supplied once per activation. Anything tracked for
    /// a screen no longer in this list (todo/done bitmaps) is disposed and dropped entirely; a
    /// blur still actively running for a dropped screen can't be interrupted (see the class
    /// remarks), but <see cref="RunBlur"/> checks against the current set itself before writing
    /// back, so its result is discarded harmlessly once it finishes. New screens not seen before
    /// start with empty state.
    /// </summary>
    public void SetActiveScreens(IReadOnlyCollection<ScreenInfo> currentScreens)
    {
        ArgumentNullException.ThrowIfNull(currentScreens);

        lock (this.sync)
        {
            foreach (var screenInfo in this.screens.Keys.Except(currentScreens).ToList())
            {
                var state = this.screens[screenInfo];
                state.Todo?.Dispose();
                state.Done?.Dispose();
                this.screens.Remove(screenInfo);
            }

            foreach (var screenInfo in currentScreens)
            {
                if (!this.screens.ContainsKey(screenInfo))
                {
                    this.screens[screenInfo] = new ScreenshotBlurState();
                }
            }
        }
    }

    /// <summary>
    /// Blurs <paramref name="image"/> on this background thread (disposing it once read from -
    /// ownership transferred in via <see cref="SetScreenshot"/>), then writes the result into
    /// this screen's "done" slot - unless <paramref name="screenInfo"/> has since been dropped
    /// via <see cref="SetActiveScreens"/>, in which case the result is discarded instead. If a
    /// newer screenshot was queued in the meantime ("todo"), starts blurring that next.
    /// </summary>
    private void RunBlur(ScreenInfo screenInfo, Bitmap image)
    {
        Bitmap blurredImage;
        using (ScreenshotBlurPipeline.CurrentTelemetry.BeginTimer(new { }, "CreateBlurredCopy"))
        using (image)
        {
            blurredImage = BlurHelper.CreateBlurredCopy(
                image, ScreenshotBlurPipeline.BlurIntensity, ScreenshotBlurPipeline.BlurSaturation, ScreenshotBlurPipeline.BlurBrightness);
        }

        lock (this.sync)
        {
            if (!this.screens.TryGetValue(screenInfo, out var state))
            {
                blurredImage.Dispose();
                return;
            }

            state.Done?.Dispose();
            state.Done = blurredImage;
            state.DoneAt = DateTime.UtcNow;
            state.BlurInProgress = false;

            if (state.Todo is not null)
            {
                var next = state.Todo;
                state.Todo = null;
                state.BlurInProgress = true;
                _ = Task.Run(() => this.RunBlur(screenInfo, next));
            }
        }
    }
}
