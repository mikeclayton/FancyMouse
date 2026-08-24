using System.Diagnostics;

namespace FancyMouse.Common.Telemetry;

/// <summary>
/// A measured span opened via <see cref="TelemetryContext.BeginTimer"/> - on Dispose, the
/// underlying <see cref="Activity"/> stops, which is when its duration becomes final and (if
/// <see cref="TelemetryContext.Start"/> has been called) gets written out. Deliberately a
/// distinct type from <see cref="TelemetryScope"/> - see its remarks for why.
/// </summary>
public sealed class TelemetryTimer : IDisposable
{
    internal TelemetryTimer(Activity? activity)
    {
        this.Activity = activity;
    }

    private Activity? Activity
    {
        get;
    }

    public void Dispose()
        => this.Activity?.Dispose();
}
