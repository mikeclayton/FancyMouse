namespace FancyMouse.Telemetry;

/// <summary>
/// Discards every <see cref="TelemetryRecord"/> it's given - for a <see cref="TelemetryContext"/>
/// that has to be started but shouldn't record anything.
/// </summary>
/// <remarks>
/// Rarely needed - a <see cref="TelemetryContext"/> that's never had
/// <see cref="TelemetryContext.Start"/> called is already a genuinely free no-op (no listener,
/// no activities), which is cheaper than starting one with this writer.
/// </remarks>
internal sealed class NullTelemetryWriter : ITelemetryWriter
{
    public static readonly NullTelemetryWriter Instance = new();

    private NullTelemetryWriter()
    {
    }

    public void Write(TelemetryRecord record)
    {
    }

    public void Flush()
    {
    }
}
