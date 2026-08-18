namespace FancyMouse.Common.Telemetry;

/// <summary>
/// Discards every <see cref="TelemetryRecord"/> it's given - the writer <see cref="Telemetry.Current"/>
/// falls back to when nothing has called <see cref="TelemetryContext.Start"/>, so touching
/// <see cref="Telemetry.Current"/> is always safe even before (or without) any real writer being
/// wired up.
/// </summary>
public sealed class NullTelemetryWriter : ITelemetryWriter
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
