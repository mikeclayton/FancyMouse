using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FancyMouse.Win32Gen.UnitTests.NativeMethods;

/// <summary>
/// Minimal in-memory <see cref="AdditionalText"/> so
/// <see cref="Win32Gen.NativeMethods.NativeMethodsTxtParser"/> can be
/// exercised without a real NativeMethods.txt file on disk.
/// </summary>
internal sealed class TestAdditionalText : AdditionalText
{
    public TestAdditionalText(string content, string path = "NativeMethods.txt")
    {
        this.Path = path;
        this.Text = SourceText.From(content);
    }

    public override string Path { get; }

    private SourceText Text { get; }

    public override SourceText GetText(CancellationToken cancellationToken = default)
        => this.Text;
}
