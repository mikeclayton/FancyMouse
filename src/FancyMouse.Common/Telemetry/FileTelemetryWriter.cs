using System.Text.Json;

namespace FancyMouse.Common.Telemetry;

/// <summary>
/// Writes each <see cref="TelemetryRecord"/> as one JSON Lines record to a file. No buffering,
/// locking, or async machinery of its own - <see cref="TelemetryAdapter"/> guarantees
/// <see cref="Write"/> is only ever called from a single background thread, one record at a
/// time. Not auto-flushed per line - relies on the normal <see cref="StreamWriter"/> buffer and
/// flushes on <see cref="Dispose"/>, trading a small risk of losing the last few buffered lines
/// on a hard crash for not paying flush cost on every single record.
/// </summary>
public sealed class FileTelemetryWriter : ITelemetryWriter, IDisposable
{
    private readonly StreamWriter writer;

    public FileTelemetryWriter(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        this.writer = new StreamWriter(filePath, append: true);
    }

    public void Write(TelemetryRecord record)
    {
        var fields = new Dictionary<string, object?>(record.Properties)
        {
            ["timestamp"] = record.Timestamp,
            ["operation"] = record.Operation,
            ["kind"] = record.Kind,
            ["durationMs"] = record.Duration.TotalMilliseconds,
            ["spanId"] = record.SpanId,
        };

        if (record.ParentSpanId is not null)
        {
            fields["parentSpanId"] = record.ParentSpanId;
        }

        this.writer.WriteLine(JsonSerializer.Serialize(fields));
    }

    public void Flush()
        => this.writer.Flush();

    public void Dispose()
        => this.writer.Dispose();
}
